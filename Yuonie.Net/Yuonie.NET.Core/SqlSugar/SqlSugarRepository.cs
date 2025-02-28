namespace Yuonie.NET;

/// <summary>
/// 使用 SqlSugar 处理数据库操作的仓储类。
/// 根据实体类类型上的特性或租户上下文自动切换数据库连接。
/// </summary>
/// <typeparam name="T">实体类类型，必须为引用类型且具有无参构造函数。</typeparam>
public class SqlSugarRepository<T> : SimpleClient<T> where T : class, new()
{
    /// <summary>
    /// 初始化 <see cref="SqlSugarRepository{T}"/> 类的新实例，
    /// 并根据实体特性和租户信息设置相应的数据库连接。
    /// </summary>
    public SqlSugarRepository(ITenant iTenant)
    {
        iTenant = App.GetRequiredService<ISqlSugarClient>().AsTenant();
        base.Context = iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);

        // 若实体贴有多库特性，则返回指定库连接
        if (typeof(T).IsDefined(typeof(TenantAttribute), false))
        {
            base.Context = iTenant.GetConnectionScopeWithAttr<T>();
            return;
        }

        // 若实体贴有日志表特性，则返回日志库连接
        if (typeof(T).IsDefined(typeof(LogTableAttribute), false))
        {
            base.Context = iTenant.IsAnyConnection(SqlSugarConst.LogConfigId)
                ? iTenant.GetConnectionScope(SqlSugarConst.LogConfigId)
                : iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            return;
        }

        // 若实体贴有系统表特性，则返回默认库连接
        if (typeof(T).IsDefined(typeof(SysTableAttribute), false))
        {
            base.Context = iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            return;
        }

        // 若未贴任何表特性或当前未登录或是默认租户Id，则返回默认库连接
        var tenantId = App.User?.FindFirst(ClaimConst.TenantId)?.Value;
        if (string.IsNullOrWhiteSpace(tenantId) || tenantId == SqlSugarConst.MainConfigId) return;

        // 根据租户Id切换库连接, 为空则返回默认库连接
        var sqlSugarScopeProvider = App.GetRequiredService<SysTenantService>().GetTenantDbConnectionScope(long.Parse(tenantId));
        if (sqlSugarScopeProvider == null) return;
        base.Context = sqlSugarScopeProvider;
    }
}
