namespace Yuonie.NET.Core;

/// <summary>
/// 全局分页查询输入参数
/// </summary>
public class BasePageInput
{
    /// <summary>
    /// 当前页码
    /// </summary>
    [DataValidation(ValidationTypes.Numeric)]
    public virtual int Page { get; set; } = 1;

    /// <summary>
    /// 页码容量
    /// </summary>
    //[Range(0, 100, ErrorMessage = "页码容量超过最大限制")]
    [DataValidation(ValidationTypes.Numeric)]
    public virtual int PageSize { get; set; } = 20;

    /// <summary>
    /// 排序 "xxx asc, xxxx desc"
    /// </summary>
    public virtual List<string> SortFieldArrary { get; set; } = new List<string>();
}