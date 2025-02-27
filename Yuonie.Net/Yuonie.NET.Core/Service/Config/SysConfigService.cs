using NewLife;

namespace Yuonie.NET.Core.Service;

/// <summary>
/// 系统参数配置服务
/// </summary>
[ApiDescriptionSettings(Order = 440)]
public class SysConfigService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysConfig> _sysConfigRep;
    private readonly SysCacheService _sysCacheService;

    public SysConfigService(SqlSugarRepository<SysConfig> sysConfigRep,
        SysCacheService sysCacheService)
    {
        _sysConfigRep = sysConfigRep;
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 获取参数配置分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取参数配置分页列表")]
    public async Task<SqlSugarPagedList<SysConfig>> Page(PageConfigInput input)
    {
        return await _sysConfigRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name?.Trim()), u => u.Name.Contains(input.Name))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Code?.Trim()), u => u.Code.Contains(input.Code))
            .WhereIF(!string.IsNullOrWhiteSpace(input.GroupCode?.Trim()), u => u.GroupCode.Equals(input.GroupCode))
            .OrderBuilder(input)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取参数配置列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取参数配置列表")]
    public async Task<List<SysConfig>> GetList()
    {
        return await _sysConfigRep.GetListAsync();
    }

    /// <summary>
    /// 增加参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加参数配置")]
    public async Task AddConfig(AddConfigInput input)
    {
        var isExist = await _sysConfigRep.IsAnyAsync(u => u.Name == input.Name || u.Code == input.Code);
        if (isExist)
            throw Oops.Oh(ErrorCodeEnum.D9000);

        await _sysConfigRep.InsertAsync(input.Adapt<SysConfig>());
    }

    /// <summary>
    /// 更新参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新参数配置")]
    public async Task UpdateConfig(UpdateConfigInput input)
    {
        var isExist = await _sysConfigRep.IsAnyAsync(u => (u.Name == input.Name || u.Code == input.Code) && u.Id != input.Id);
        if (isExist)
            throw Oops.Oh(ErrorCodeEnum.D9000);

        var config = input.Adapt<SysConfig>();
        await _sysConfigRep.AsUpdateable(config).IgnoreColumns(true).ExecuteCommandAsync();

        _sysCacheService.Remove(config.Code);
    }

    /// <summary>
    /// 删除参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除参数配置")]
    public async Task DeleteConfig(DeleteConfigInput input)
    {
        var config = await _sysConfigRep.GetFirstAsync(u => u.Id == input.Id);
        if (config.SysFlag == YesNoEnum.Y) // 禁止删除系统参数
            throw Oops.Oh(ErrorCodeEnum.D9001);

        await _sysConfigRep.DeleteAsync(config);

        _sysCacheService.Remove(config.Code);
    }

    /// <summary>
    /// 批量删除参数配置
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    [DisplayName("批量删除参数配置")]
    public async Task BatchDeleteConfig(List<long> ids)
    {
        foreach (var id in ids)
        {
            var config = await _sysConfigRep.GetFirstAsync(u => u.Id == id);
            if (config.SysFlag == YesNoEnum.Y) // 禁止删除系统参数
                continue;

            await _sysConfigRep.DeleteAsync(config);

            _sysCacheService.Remove(config.Code);
        }
    }

    /// <summary>
    /// 获取参数配置详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取参数配置详情")]
    public async Task<SysConfig> GetDetail([FromQuery] ConfigInput input)
    {
        return await _sysConfigRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<T> GetConfigValue<T>(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return default;

        var value = _sysCacheService.Get<string>(code);
        if (string.IsNullOrEmpty(value))
        {
            var config = await _sysConfigRep.GetFirstAsync(u => u.Code == code);
            value = config != null ? config.Value : default;
            _sysCacheService.Set(code, value);
        }
        if (string.IsNullOrWhiteSpace(value)) return default;
        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// 获取分组列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取分组列表")]
    public async Task<List<string>> GetGroupList()
    {
        return await _sysConfigRep.AsQueryable().GroupBy(u => u.GroupCode).Select(u => u.GroupCode).ToListAsync();
    }

    /// <summary>
    /// 获取 Token 过期时间
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<int> GetTokenExpire()
    {
        var tokenExpireStr = await GetConfigValue<string>(CommonConst.SysTokenExpire);
        _ = int.TryParse(tokenExpireStr, out var tokenExpire);
        return tokenExpire == 0 ? 20 : tokenExpire;
    }

    /// <summary>
    /// 获取 RefreshToken 过期时间
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<int> GetRefreshTokenExpire()
    {
        var refreshTokenExpireStr = await GetConfigValue<string>(CommonConst.SysRefreshTokenExpire);
        _ = int.TryParse(refreshTokenExpireStr, out var refreshTokenExpire);
        return refreshTokenExpire == 0 ? 40 : refreshTokenExpire;
    }
}