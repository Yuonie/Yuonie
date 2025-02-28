namespace Yuonie.NET.Core.Service;

/// <summary>
/// 系统租户管理服务
/// </summary>
[ApiDescriptionSettings(Order = 390)]
public class SysTenantService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysTenant> _sysTenantRep;
    private readonly SysCacheService _sysCacheService;

    public SysTenantService(SqlSugarRepository<SysTenant> sysTenantRep,
                                               SysCacheService sysCacheService)
    {
        _sysTenantRep = sysTenantRep;
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 获取库隔离的租户列表
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<SysTenant>> GetTenantDbList()
    {
        return await _sysTenantRep.GetListAsync(u => u.TenantType == TenantTypeEnum.Db && u.Status == StatusEnum.Enable);
    }
    /// <summary>
    /// 获取租户数据库连接
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId)
    {
        var iTenant = _sysTenantRep.AsTenant();

        // 若已存在租户库连接，则直接返回
        if (iTenant.IsAnyConnection(tenantId.ToString()))
            return iTenant.GetConnectionScope(tenantId.ToString());

        lock (iTenant)
        {
            // 从缓存里面获取租户信息
            var tenant = _sysCacheService.Get<List<SysTenant>>(CacheConst.KeyTenant)?.First(u => u.Id == tenantId);
            if (tenant == null) return null;

            // 获取默认库连接配置
            var dbOptions = App.GetOptions<DbConnectionOptions>();
            var mainConnConfig = dbOptions.ConnectionConfigs.First(u => u.ConfigId.ToString() == SqlSugarConst.MainConfigId);

            // 设置租户库连接配置
            var tenantConnConfig = new DbConnectionConfig
            {
                ConfigId = tenant.Id.ToString(),
                DbType = tenant.DbType,
                IsAutoCloseConnection = true,
                ConnectionString = CryptogramUtil.SM2Decrypt(tenant.Connection), // 对租户库连接进行SM2解密
                DbSettings = new DbSettings()
                {
                    EnableUnderLine = mainConnConfig.DbSettings.EnableUnderLine,
                },
                SlaveConnectionConfigs = JSON.IsValid(tenant.SlaveConnections) ? JSON.Deserialize<List<SlaveConnectionConfig>>(tenant.SlaveConnections) : null // 从库连接配置
            };
            iTenant.AddConnection(tenantConnConfig);

            var sqlSugarScopeProvider = iTenant.GetConnectionScope(tenantId.ToString());
            SqlSugarSetup.SetDbConfig(tenantConnConfig);
            SqlSugarSetup.SetDbAop(sqlSugarScopeProvider, dbOptions.EnableConsoleSql);

            return sqlSugarScopeProvider;
        }
    }
}