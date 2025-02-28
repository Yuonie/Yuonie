namespace Yuonie.NET.Core
{
    /// <summary>
    /// SQLSugar 数据过滤工具类，用于动态配置和管理全局查询过滤器。
    /// </summary>
    public static class SqlSugarFilter
    {
        /// <summary>
        /// 缓存全局查询过滤器（内存缓存）。
        /// </summary>
        private static readonly ICache _cache = Cache.Default;

        /// <summary>
        /// 删除指定用户的机构缓存，包括用户数据范围的过滤器。
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dbConfigId">数据库配置ID</param>
        public static void DeleteUserOrgCache(long userId, string dbConfigId)
        {
            var sysCacheService = App.GetService<SysCacheService>();

            // 删除用户机构相关的缓存。
            sysCacheService.Remove($"{CacheConst.KeyUserOrg}{userId}");
            sysCacheService.Remove($"{CacheConst.KeyRoleMaxDataScope}{userId}");
            _cache.Remove($"db:{dbConfigId}:orgList:{userId}");
        }

        /// <summary>
        /// 配置用户机构集合过滤器，用于过滤符合机构权限的数据。
        /// </summary>
        /// <param name="db">SQLSugar 数据库上下文对象</param>
        public static void SetOrgEntityFilter(SqlSugarScopeProvider db)
        {
            // 如果用户仅拥有“本人数据”权限，直接返回。
            if (SetDataScopeFilter(db) == (int)DataScopeEnum.Self) return;

            var userId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return;

            // 构造缓存键值。
            var cacheKey = $"db:{db.CurrentConnectionConfig.ConfigId}:orgList:{userId}";

            // 从缓存中获取过滤器，如果不存在则构造并缓存。
            if (!_cache.TryGetValue(cacheKey, out ConcurrentDictionary<Type, LambdaExpression> orgFilter))
            {
                // 获取用户所属机构列表。
                var orgIds = App.GetService<SysOrgService>().GetUserOrgIdList().GetAwaiter().GetResult();
                if (orgIds == null || orgIds.Count == 0) return;

                // 获取有效的业务实体类型。
                var entityTypes = GetEffectiveEntityTypes();
                orgFilter = new ConcurrentDictionary<Type, LambdaExpression>();

                foreach (var entityType in entityTypes)
                {
                    // 验证实体是否属于当前租户数据库。
                    if (!IsValidTenantEntity(entityType, db)) continue;

                    // 创建表达式过滤器：u => orgIds.Contains(u.CreateOrgId ?? 0)
                    var param = Expression.Parameter(entityType, "u");
                    var property = Expression.Property(param, nameof(EntityBaseData.CreateOrgId));
                    var coalesce = Expression.Coalesce(property, Expression.Constant(0L));
                    var contains = Expression.Call(Expression.Constant(orgIds), "Contains", null, coalesce);
                    var lambda = Expression.Lambda(contains, param);

                    // 添加过滤器到数据库上下文。
                    db.QueryFilter.AddTableFilter(entityType, lambda);
                    orgFilter.TryAdd(entityType, lambda);
                }

                // 缓存过滤器。
                _cache.Set(cacheKey, orgFilter);
            }
            else
            {
                // 如果过滤器已缓存，则直接添加到数据库上下文。
                foreach (var filter in orgFilter)
                    db.QueryFilter.AddTableFilter(filter.Key, filter.Value);
            }
        }

        /// <summary>
        /// 配置用户“仅本人数据”过滤器。
        /// </summary>
        /// <param name="db">SQLSugar 数据库上下文对象</param>
        private static int SetDataScopeFilter(SqlSugarScopeProvider db)
        {
            var maxDataScope = (int)DataScopeEnum.All;

            var userId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return maxDataScope;

            // 从缓存中获取用户数据范围。
            maxDataScope = App.GetService<SysCacheService>().Get<int>(CacheConst.KeyRoleMaxDataScope + userId);
            if (maxDataScope != (int)DataScopeEnum.Self) return maxDataScope;

            var cacheKey = $"db:{db.CurrentConnectionConfig.ConfigId}:dataScope:{userId}";

            // 如果过滤器未缓存，则构造新的过滤器。
            if (!_cache.TryGetValue(cacheKey, out ConcurrentDictionary<Type, LambdaExpression> dataScopeFilter))
            {
                var entityTypes = GetEffectiveEntityTypes();
                dataScopeFilter = new ConcurrentDictionary<Type, LambdaExpression>();

                foreach (var entityType in entityTypes)
                {
                    if (!IsValidTenantEntity(entityType, db)) continue;

                    // 创建表达式过滤器：u => u.CreateUserId == userId
                    var param = Expression.Parameter(entityType, "u");
                    var property = Expression.Property(param, nameof(EntityBaseData.CreateUserId));
                    var equal = Expression.Equal(property, Expression.Constant(userId));
                    var lambda = Expression.Lambda(equal, param);

                    // 添加过滤器到数据库上下文。
                    db.QueryFilter.AddTableFilter(entityType, lambda);
                    dataScopeFilter.TryAdd(entityType, lambda);
                }

                // 缓存过滤器。
                _cache.Set(cacheKey, dataScopeFilter);
            }
            else
            {
                // 如果过滤器已缓存，则直接添加到数据库上下文。
                foreach (var filter in dataScopeFilter)
                    db.QueryFilter.AddTableFilter(filter.Key, filter.Value);
            }

            return maxDataScope;
        }

        /// <summary>
        /// 配置自定义过滤器，用于灵活扩展业务逻辑。
        /// </summary>
        /// <param name="db">SQLSugar 数据库上下文对象</param>
        public static void SetCustomEntityFilter(SqlSugarScopeProvider db)
        {
            var userId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
            var cacheKey = $"db:{db.CurrentConnectionConfig.ConfigId}:custom:{userId}";

            // 检查缓存中是否存在自定义过滤器。
            if (!_cache.TryGetValue(cacheKey, out List<TableFilterItem<object>> tableFilterItemList))
            {
                var entityFilterTypes = GetEntityFilterTypes();
                tableFilterItemList = new List<TableFilterItem<object>>();

                foreach (var entityFilter in entityFilterTypes)
                {
                    var instance = Activator.CreateInstance(entityFilter);
                    var entityFilterMethod = entityFilter.GetMethod("AddEntityFilter");
                    var entityFilters = ((IList)entityFilterMethod?.Invoke(instance, null))?.Cast<TableFilterItem<object>>();

                    if (entityFilters == null) continue;

                    foreach (var filterItem in entityFilters)
                    {
                        var entityType = filterItem.GetType().GetProperty("type", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(filterItem, null) as Type;
                        if (!IsValidTenantEntity(entityType, db)) continue;

                        tableFilterItemList.Add(filterItem);
                        db.QueryFilter.Add(filterItem);
                    }
                }

                _cache.Set(cacheKey, tableFilterItemList);
            }
            else
            {
                // 如果缓存中已有过滤器，则直接添加。
                foreach (var filterItem in tableFilterItemList)
                    db.QueryFilter.Add(filterItem);
            }
        }

        /// <summary>
        /// 获取有效的业务实体类型集合。
        /// </summary>
        private static IEnumerable<Type> GetEffectiveEntityTypes() =>
            App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsSubclassOf(typeof(EntityBaseData)));

        /// <summary>
        /// 验证实体是否属于当前租户数据库。
        /// </summary>
        private static bool IsValidTenantEntity(Type entityType, SqlSugarScopeProvider db)
        {
            var tenantAttr = entityType.GetCustomAttribute<TenantAttribute>();
            return tenantAttr == null || db.CurrentConnectionConfig.ConfigId.ToString() == tenantAttr.configId.ToString();
        }

        /// <summary>
        /// 获取实现了 IEntityFilter 接口的类型集合。
        /// </summary>
        private static IEnumerable<Type> GetEntityFilterTypes() =>
            App.EffectiveTypes.Where(u => u.IsClass && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(IEntityFilter))));
    }

    /// <summary>
    /// 自定义实体过滤器接口，用于扩展实体过滤逻辑。
    /// </summary>
    public interface IEntityFilter
    {
        /// <summary>
        /// 添加实体过滤器的方法。
        /// </summary>
        /// <returns>返回过滤器列表。</returns>
        IEnumerable<TableFilterItem<object>> AddEntityFilter();
    }
}
