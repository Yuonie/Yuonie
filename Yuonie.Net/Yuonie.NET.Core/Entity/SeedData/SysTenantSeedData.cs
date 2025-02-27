namespace Yuonie.NET.Entity.SeedData;

/// <summary>
/// 系统租户表种子数据
/// </summary>
public class SysTenantSeedData : ISqlSugarEntitySeedData<SysTenant>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysTenant> HasData()
    {
        var dbConnectionOptions = App.GetOptions<DbConnectionOptions>();
        var defaultDbConfig = dbConnectionOptions.ConnectionConfigs
            .Where(a => a.ConnId == dbConnectionOptions.MainDB).FirstOrDefault();

        return new[]
        {
            new SysTenant{ Id=1300000000001, OrgId=1300000000101, UserId=1300000000111, Host="www.dilon.vip", TenantType=TenantTypeEnum.Id, DbType=defaultDbConfig.DbType, Connection=defaultDbConfig.ConnectionString, ConfigId=defaultDbConfig.ConfigId?.ToString(), Remark="系统默认", CreateTime=DateTime.Parse("2024-10-30 00:00:00") },
        };
    }
}