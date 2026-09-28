using System.Linq.Expressions;
using ERP.Domain;
using ERP.Shared.DTOs;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Extensions;

/// <summary>
/// Whitelist of sortable fields for one list endpoint. Keys are the camelCase names the client sends.
/// </summary>
public sealed class SortMap<T> where T : BaseEntity
{
    private readonly Dictionary<string, Expression<Func<T, object?>>> _fields = new(StringComparer.OrdinalIgnoreCase);

    public SortMap(string defaultKey)
    {
        DefaultKey = defaultKey;
    }

    public string DefaultKey { get; }
    public IEnumerable<string> Keys => _fields.Keys;

    public SortMap<T> Add(string key, Expression<Func<T, object?>> selector)
    {
        _fields[key] = selector;
        return this;
    }

    public Expression<Func<T, object?>> Resolve(string? key)
    {
        var effectiveKey = string.IsNullOrWhiteSpace(key) ? DefaultKey : key;
        if (_fields.TryGetValue(effectiveKey, out var selector)) return selector;

        throw new ValidationException(
            $"Cannot sort by '{effectiveKey}'.",
            new List<string> { $"Allowed sort fields: {string.Join(", ", _fields.Keys)}." });
    }
}

public static class PagingExtensions
{
    /// <summary>
    /// Sorts (whitelisted field, then Id for a stable order), counts and pages a query in two round trips.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        SortMap<T> sortMap,
        CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var selector = sortMap.Resolve(request.SortBy);
        var ordered = request.SortDescending
            ? query.OrderByDescending(selector).ThenBy(e => e.Id)
            : query.OrderBy(selector).ThenBy(e => e.Id);

        var pageNumber = request.NormalizedPageNumber;
        var pageSize = request.NormalizedPageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }

}
