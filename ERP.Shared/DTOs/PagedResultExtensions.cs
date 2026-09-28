namespace ERP.Shared.DTOs;

public static class PagedResultExtensions
{
    public static PagedResult<TOut> Map<TIn, TOut>(this PagedResult<TIn> page, Func<TIn, TOut> map) => new()
    {
        Items = page.Items.Select(map).ToList(),
        TotalCount = page.TotalCount,
        PageNumber = page.PageNumber,
        PageSize = page.PageSize,
    };
}
