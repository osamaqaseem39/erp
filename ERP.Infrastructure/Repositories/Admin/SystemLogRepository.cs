using ERP.Domain.Admin;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Extensions;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Admin;

public class SystemLogRepository : TenantScopedRepositoryBase, ISystemLogRepository
{
    private static readonly SortMap<SystemLog> SortFields = new SortMap<SystemLog>("timestamp")
        .Add("timestamp", l => l.Timestamp)
        .Add("logLevel", l => l.LogLevel)
        .Add("source", l => l.Source);

    public SystemLogRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider)
    {
    }

    private IQueryable<SystemLog> Logs => Scoped(Context.SystemLogs.AsNoTracking());

    public async Task<SystemLog?> GetByIdAsync(Guid id) => await Logs.FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<SystemLog>> GetAllAsync() =>
        await Logs.OrderByDescending(l => l.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<IEnumerable<SystemLog>> GetByLevelAsync(string logLevel) =>
        await Logs.Where(l => l.LogLevel == logLevel).OrderByDescending(l => l.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<IEnumerable<SystemLog>> GetByUserAsync(Guid userId) =>
        await Logs.Where(l => l.UserId == userId).OrderByDescending(l => l.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        await Logs.Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate).OrderByDescending(l => l.Timestamp).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<PagedResult<SystemLog>> GetPagedAsync(SystemLogPagedQuery query, bool allTenants = false, CancellationToken cancellationToken = default)
    {
        var logs = allTenants ? Context.SystemLogs.AsNoTracking() : Logs;
        if (!string.IsNullOrWhiteSpace(query.LogLevel)) logs = logs.Where(l => l.LogLevel == query.LogLevel);
        if (!string.IsNullOrWhiteSpace(query.Source)) logs = logs.Where(l => l.Source == query.Source);
        if (query.From.HasValue) logs = logs.Where(l => l.Timestamp >= query.From);
        if (query.To.HasValue) logs = logs.Where(l => l.Timestamp <= query.To);
        var term = query.NormalizedSearchTerm;
        if (term != null) logs = logs.Where(l => l.Message.Contains(term));

        if (string.IsNullOrWhiteSpace(query.SortBy)) query.SortDescending = true;
        return await logs.ToPagedResultAsync(query, SortFields, cancellationToken);
    }
}
