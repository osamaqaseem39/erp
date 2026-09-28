namespace ERP.Shared.DTOs;

/// <summary>
/// Paging, search and sort parameters bound from the query string of a "/paged" list endpoint.
/// Unlike <see cref="BaseQuery"/> it carries no TenantId: the tenant always comes from the authenticated user.
/// </summary>
public class PagedRequest
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 200;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public string? SearchTerm { get; set; }
    /// <summary>A key from the endpoint's sort whitelist (camelCase field name). Unknown keys are rejected.</summary>
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }

    public int NormalizedPageNumber => Math.Max(1, PageNumber);
    public int NormalizedPageSize => Math.Clamp(PageSize, 1, MaxPageSize);
    public string? NormalizedSearchTerm => string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim();
}
