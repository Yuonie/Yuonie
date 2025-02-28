namespace Yuonie.NET.Core;

public static class SqlSugarOrderBuilder
{
    /// <summary>
    /// 排序扩展
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageInput"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> OrderBuilder<T>(this ISugarQueryable<T> queryable, BasePageInput pageInput)
    {
        var items = queryable.OrderByIF(pageInput.SortFieldArrary.Count() > 0, string.Join(",", pageInput.SortFieldArrary));
        return items;
    }
}
