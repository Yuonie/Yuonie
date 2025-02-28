﻿namespace Yuonie.NET.Entity;

/// <summary>
/// 系统角色菜单表
/// </summary>
[SugarTable(null, "系统角色菜单表")]
[SysTable]
public class SysRoleMenu : EntityBaseId
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id")]
    public long RoleId { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id")]
    public long MenuId { get; set; }

    /// <summary>
    /// 菜单
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(MenuId))]
    public SysMenu SysMenu { get; set; }
}