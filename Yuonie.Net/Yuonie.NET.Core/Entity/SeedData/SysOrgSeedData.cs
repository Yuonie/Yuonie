﻿namespace Yuonie.NET.Entity.SeedData;

/// <summary>
/// 系统机构表种子数据
/// </summary>
public class SysOrgSeedData : ISqlSugarEntitySeedData<SysOrg>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysOrg> HasData()
    {
        return new[]
        {
            new SysOrg{ Id=1300000000101, Pid=0, Name="大名科技", Code="1001", Type="101", Level=1, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="大名科技", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000102, Pid=1300000000101, Name="市场部", Code="100101", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="市场部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000103, Pid=1300000000101, Name="研发部", Code="100102", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="研发部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000104, Pid=1300000000101, Name="财务部", Code="100103", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="财务部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000105, Pid=1300000000104, Name="财务部1", Code="10010301", Level=3, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="财务部1", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000106, Pid=1300000000104, Name="财务部2", Code="10010302", Level=3, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="财务部2", TenantId=1300000000001 },

            new SysOrg{ Id=1300000000201, Pid=0, Name="分公司1", Code="1002", Type="201", Level=1, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="分公司1", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000202, Pid=1300000000201, Name="市场部", Code="100201", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="市场部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000203, Pid=1300000000201, Name="研发部", Code="100202", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="研发部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000204, Pid=1300000000201, Name="财务部", Code="100203", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="财务部", TenantId=1300000000001 },

            new SysOrg{ Id=1300000000301, Pid=0, Name="分公司2", Code="1003", Type="201", Level=1, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="分公司2", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000302, Pid=1300000000301, Name="市场部", Code="100301", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="市场部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000303, Pid=1300000000301, Name="研发部", Code="100302", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="市场部", TenantId=1300000000001 },
            new SysOrg{ Id=1300000000304, Pid=1300000000301, Name="财务部", Code="100303", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="市场部", TenantId=1300000000001 },
        };
    }
}