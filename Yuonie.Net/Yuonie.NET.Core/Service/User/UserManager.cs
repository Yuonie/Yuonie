using Microsoft.AspNetCore.Http;

namespace Yuonie.NET.Core.Service;

/// <summary>
/// 当前登录用户
/// </summary>
public class UserManager : IScoped
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId => (_httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.UserId)?.Value).ToLong();

    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId => (_httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.TenantId)?.Value).ToLong();

    /// <summary>
    /// 用户账号
    /// </summary>
    public string Account => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.Account)?.Value;

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.RealName)?.Value;

    /// <summary>
    /// 是否超级管理员
    /// </summary>
    public bool SuperAdmin => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.AccountType)?.Value == ((int)AccountTypeEnum.SuperAdmin).ToString();

    /// <summary>
    /// 组织机构Id
    /// </summary>
    public long OrgId => (_httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.OrgId)?.Value).ToLong();

    /// <summary>
    /// 微信OpenId
    /// </summary>
    public string OpenId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.OpenId)?.Value;

    public UserManager(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
}