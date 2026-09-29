using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Extensions;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Admin;

public class AuditLogRepository : TenantScopedRepositoryBase, IAuditLogRepository
{
    private static readonly SortMap<AuditLog> SortFields = new SortMap<AuditLog>("timestamp")
        .Add("timestamp", a => a.Timestamp)
        .Add("entityName", a => a.EntityName)
        .Add("action", a => a.Action);

    public AuditLogRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider)
    {
    }

    private IQueryable<AuditLog> Logs => Scoped(Context.AuditLogs.AsNoTracking());

    public async Task<AuditLog?> GetByIdAsync(Guid id) => await Logs.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<AuditLog>> GetAllAsync() =>
        await Logs.OrderByDescending(a => a.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId) =>
        await Logs.Where(a => a.EntityName == entityName && a.EntityId == entityId).OrderByDescending(a => a.Timestamp).ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId) =>
        await Logs.Where(a => a.UserId == userId).OrderByDescending(a => a.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action) =>
        await Logs.Where(a => a.Action == action).OrderByDescending(a => a.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        await Logs.Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate).OrderByDescending(a => a.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogPagedQuery query, bool allTenants = false, CancellationToken cancellationToken = default)
    {
        // allTenants is only ever passed as true by the API for SuperAdmin callers.
        var logs = allTenants ? Context.AuditLogs.AsNoTracking() : Logs;
        if (!string.IsNullOrWhiteSpace(query.EntityName)) logs = logs.Where(a => a.EntityName == query.EntityName);
        if (!string.IsNullOrWhiteSpace(query.EntityId)) logs = logs.Where(a => a.EntityId == query.EntityId);
        if (!string.IsNullOrWhiteSpace(query.Action)) logs = logs.Where(a => a.Action == query.Action);
        if (query.UserId.HasValue) logs = logs.Where(a => a.UserId == query.UserId);
        if (query.From.HasValue) logs = logs.Where(a => a.Timestamp >= query.From);
        if (query.To.HasValue) logs = logs.Where(a => a.Timestamp <= query.To);
        var term = query.NormalizedSearchTerm;
        if (term != null) logs = logs.Where(a => a.EntityName.Contains(term) || a.EntityId.Contains(term) || a.Action.Contains(term));

        // Newest first unless the caller chose a sort.
        if (string.IsNullOrWhiteSpace(query.SortBy)) query.SortDescending = true;
        return await logs.ToPagedResultAsync(query, SortFields, cancellationToken);
    }
}
