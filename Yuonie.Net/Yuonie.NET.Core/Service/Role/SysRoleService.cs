﻿namespace Yuonie.NET.Core.Service;

/// <summary>
/// 系统角色服务
/// </summary>
[ApiDescriptionSettings(Order = 480)]
public class SysRoleService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SqlSugarRepository<SysRole> _sysRoleRep;
    private readonly SysRoleOrgService _sysRoleOrgService;
    private readonly SysRoleMenuService _sysRoleMenuService;
    private readonly SysOrgService _sysOrgService;
    private readonly SysUserRoleService _sysUserRoleService;

    public SysRoleService(UserManager userManager,
        SqlSugarRepository<SysRole> sysRoleRep,
        SysRoleOrgService sysRoleOrgService,
        SysRoleMenuService sysRoleMenuService,
        SysOrgService sysOrgService,
        SysUserRoleService sysUserRoleService)
    {
        _userManager = userManager;
        _sysRoleRep = sysRoleRep;
        _sysRoleOrgService = sysRoleOrgService;
        _sysRoleMenuService = sysRoleMenuService;
        _sysOrgService = sysOrgService;
        _sysUserRoleService = sysUserRoleService;
    }

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取角色分页列表")]
    public async Task<SqlSugarPagedList<SysRole>> Page(PageRoleInput input)
    {
        return await _sysRoleRep.AsQueryable()
            .WhereIF(!_userManager.SuperAdmin, u => u.CreateUserId == _userManager.UserId) // 若非超管，则只能操作自己创建的角色
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code.Contains(input.Code))
            .OrderBy(u => u.OrderNo)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取角色列表")]
    public async Task<List<RoleOutput>> GetList()
    {
        // 当前用户已拥有的角色集合
        var roleIdList = _userManager.SuperAdmin ? null : await _sysUserRoleService.GetUserRoleIdList(_userManager.UserId);

        return await _sysRoleRep.AsQueryable()
            .WhereIF(roleIdList != null, u => u.CreateUserId == _userManager.UserId || roleIdList.Contains(u.Id)) // 若非超管，则只显示自己创建和已拥有的角色
            .OrderBy(u => u.OrderNo).Select<RoleOutput>().ToListAsync();
    }

    /// <summary>
    /// 增加角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加角色")]
    public async Task AddRole(AddRoleInput input)
    {
        if (await _sysRoleRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
            throw Oops.Oh(ErrorCodeEnum.D1006);

        var newRole = await _sysRoleRep.AsInsertable(input.Adapt<SysRole>()).ExecuteReturnEntityAsync();
        input.Id = newRole.Id;
        await UpdateRoleMenu(input);
    }

    /// <summary>
    /// 更新角色菜单权限
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleMenu(AddRoleInput input)
    {
        if (input.MenuIdList == null || input.MenuIdList.Count < 1)
            return;

        // 将父节点为0的菜单排除，防止前端全选异常
        var pMenuIds = await _sysRoleRep.ChangeRepository<SqlSugarRepository<SysMenu>>().AsQueryable().Where(u => input.MenuIdList.Contains(u.Id) && u.Pid == 0).ToListAsync(u => u.Id);
        var menuIds = input.MenuIdList.Except(pMenuIds); // 差集
        await GrantMenu(new RoleMenuInput()
        {
            Id = input.Id,
            MenuIdList = menuIds.ToList()
        });
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新角色")]
    public async Task UpdateRole(UpdateRoleInput input)
    {
        if (await _sysRoleRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
            throw Oops.Oh(ErrorCodeEnum.D1006);

        await _sysRoleRep.AsUpdateable(input.Adapt<SysRole>()).IgnoreColumns(true)
            .IgnoreColumns(u => new { u.DataScope }).ExecuteCommandAsync();

        await UpdateRoleMenu(input);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除角色")]
    public async Task DeleteRole(DeleteRoleInput input)
    {
        // 禁止删除系统管理员角色
        var sysRole = await _sysRoleRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        if (sysRole.Code == CommonConst.SysAdminRole)
            throw Oops.Oh(ErrorCodeEnum.D1019);

        // 若角色有用户则禁止删除
        var userIds = await _sysUserRoleService.GetUserIdList(input.Id);
        if (userIds != null && userIds.Count > 0)
            throw Oops.Oh(ErrorCodeEnum.D1025);

        await _sysRoleRep.DeleteAsync(sysRole);

        // 级联删除角色机构数据
        await _sysRoleOrgService.DeleteRoleOrgByRoleId(sysRole.Id);

        // 级联删除用户角色数据
        await _sysUserRoleService.DeleteUserRoleByRoleId(sysRole.Id);

        // 级联删除角色菜单数据
        await _sysRoleMenuService.DeleteRoleMenuByRoleId(sysRole.Id);
    }

    /// <summary>
    /// 授权角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("授权角色菜单")]
    public async Task GrantMenu(RoleMenuInput input)
    {
        await _sysRoleMenuService.GrantRoleMenu(input);
    }

    /// <summary>
    /// 授权角色数据范围
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("授权角色数据范围")]
    public async Task GrantDataScope(RoleOrgInput input)
    {
        // 删除与该角色相关的用户机构缓存
        var userIdList = await _sysUserRoleService.GetUserIdList(input.Id);
        foreach (var userId in userIdList)
        {
            SqlSugarFilter.DeleteUserOrgCache(userId, _sysRoleRep.Context.CurrentConnectionConfig.ConfigId.ToString());
        }

        var role = await _sysRoleRep.GetFirstAsync(u => u.Id == input.Id);
        var dataScope = input.DataScope;
        if (!_userManager.SuperAdmin)
        {
            // 非超级管理员没有全部数据范围权限
            if (dataScope == (int)DataScopeEnum.All)
                throw Oops.Oh(ErrorCodeEnum.D1016);

            // 若数据范围自定义，则判断授权数据范围是否有权限
            if (dataScope == (int)DataScopeEnum.Define)
            {
                var grantOrgIdList = input.OrgIdList;
                if (grantOrgIdList.Count > 0)
                {
                    var orgIdList = await _sysOrgService.GetUserOrgIdList();
                    if (orgIdList.Count < 1)
                        throw Oops.Oh(ErrorCodeEnum.D1016);
                    else if (!grantOrgIdList.All(u => orgIdList.Any(c => c == u)))
                        throw Oops.Oh(ErrorCodeEnum.D1016);
                }
            }
        }
        role.DataScope = (DataScopeEnum)dataScope;
        await _sysRoleRep.AsUpdateable(role).UpdateColumns(u => new { u.DataScope }).ExecuteCommandAsync();
        await _sysRoleOrgService.GrantRoleOrg(input);
    }

    /// <summary>
    /// 根据角色Id获取菜单Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据角色Id获取菜单Id集合")]
    public async Task<List<long>> GetOwnMenuList([FromQuery] RoleInput input)
    {
        return await _sysRoleMenuService.GetRoleMenuIdList(new List<long> { input.Id });
    }

    /// <summary>
    /// 根据角色Id获取机构Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据角色Id获取机构Id集合")]
    public async Task<List<long>> GetOwnOrgList([FromQuery] RoleInput input)
    {
        return await _sysRoleOrgService.GetRoleOrgIdList(new List<long> { input.Id });
    }

    /// <summary>
    /// 设置角色状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置角色状态")]
    public async Task<int> SetStatus(RoleInput input)
    {
        if (!Enum.IsDefined(typeof(StatusEnum), input.Status))
            throw Oops.Oh(ErrorCodeEnum.D3005);

        return await _sysRoleRep.AsUpdateable()
            .SetColumns(u => u.Status == input.Status)
            .Where(u => u.Id == input.Id)
            .ExecuteCommandAsync();
    }
}