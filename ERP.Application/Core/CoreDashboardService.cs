using ERP.Infrastructure.Database;
using ERP.Infrastructure.Logging;
using ERP.Shared.DTOs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Core;

public interface ICoreDashboardService
{
    Task<CoreDashboardDto> GetSummaryAsync(Guid tenantId, Guid? companyId, bool includePlatform, CancellationToken cancellationToken = default);
}

/// <summary>
/// "System health" for the Core dashboard. Every figure is a query over real rows; there are no
/// sample values. Each widget is computed separately so one failure only affects its own card.
/// </summary>
public class CoreDashboardService : ICoreDashboardService
{
    private static readonly string[] ConfigEntities = ["SystemSetting", "Company", "DocumentSequence"];

    private readonly ERPDbContext _db;
    private readonly TimeProvider _clock;
    private readonly ILogger<CoreDashboardService> _logger;

    public CoreDashboardService(ERPDbContext db, TimeProvider clock, ILogger<CoreDashboardService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CoreDashboardDto> GetSummaryAsync(Guid tenantId, Guid? companyId, bool includePlatform, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        return new CoreDashboardDto
        {
            Users = await Widget("users", () => UsersAsync(tenantId, now, cancellationToken)),
            Security = await Widget("security", () => SecurityAsync(tenantId, now, cancellationToken)),
            Errors = await Widget("errors", () => ErrorsAsync(tenantId, now, cancellationToken)),
            ConfigChanges = await Widget("configChanges", () => ConfigChangesAsync(tenantId, cancellationToken)),
            Setup = await Widget("setup", () => SetupAsync(tenantId, companyId, cancellationToken)),
            Tenants = includePlatform ? await Widget("tenants", () => TenantsAsync(cancellationToken)) : null,
        };
    }

    private async Task<WidgetResult<T>> Widget<T>(string name, Func<Task<T>> compute)
    {
        try
        {
            return new WidgetResult<T> { Data = await compute() };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Core dashboard widget {Widget} failed", name);
            return new WidgetResult<T> { Error = "This figure couldn't be calculated right now." };
        }
    }

    private async Task<UserActivityWidget> UsersAsync(Guid tenantId, DateTime now, CancellationToken ct)
    {
        var users = _db.Users.AsNoTracking().Where(u => u.TenantId == tenantId && u.IsActive);
        // Sign-in stamps LastLoginDate (older rows may only have LastLoginAt); per-tenant lists are small.
        var logins = await users.Select(u => new { u.LastLoginDate, u.LastLoginAt, u.IsLocked, u.LockedUntil }).ToListAsync(ct);
        var last = logins.Select(u => u.LastLoginDate ?? u.LastLoginAt).ToList();
        return new UserActivityWidget
        {
            TotalUsers = logins.Count,
            ActiveLast7Days = last.Count(d => d >= now.AddDays(-7)),
            ActivePrevious7Days = last.Count(d => d >= now.AddDays(-14) && d < now.AddDays(-7)),
            LockedAccounts = logins.Count(u => u.IsLocked && u.LockedUntil > now),
        };
    }

    private async Task<SecurityWidget> SecurityAsync(Guid tenantId, DateTime now, CancellationToken ct)
    {
        var security = _db.SystemLogs.AsNoTracking().Where(l => l.TenantId == tenantId && l.Source == SystemLogSources.Security);
        return new SecurityWidget
        {
            FailedSignInsLast24h = await security.CountAsync(l => l.Message.StartsWith("Failed sign-in") && l.Timestamp >= now.AddHours(-24), ct),
            FailedSignInsPrevious24h = await security.CountAsync(l => l.Message.StartsWith("Failed sign-in") && l.Timestamp >= now.AddHours(-48) && l.Timestamp < now.AddHours(-24), ct),
            Lockouts7Days = await security.CountAsync(l => l.Message.StartsWith("Account locked") && l.Timestamp >= now.AddDays(-7), ct),
        };
    }

    private async Task<ErrorsWidget> ErrorsAsync(Guid tenantId, DateTime now, CancellationToken ct)
    {
        var errors = _db.SystemLogs.AsNoTracking().Where(l => l.TenantId == tenantId && l.LogLevel == "Error");
        return new ErrorsWidget
        {
            ErrorsLast24h = await errors.CountAsync(l => l.Timestamp >= now.AddHours(-24), ct),
            ErrorsPrevious24h = await errors.CountAsync(l => l.Timestamp >= now.AddHours(-48) && l.Timestamp < now.AddHours(-24), ct),
            Latest = await errors.OrderByDescending(l => l.Timestamp).Take(5)
                .Select(l => new LogEntrySummary { Id = l.Id, Timestamp = l.Timestamp, Message = l.Message, Level = l.LogLevel })
                .ToListAsync(ct),
        };
    }

    private async Task<List<ConfigChangeSummary>> ConfigChangesAsync(Guid tenantId, CancellationToken ct)
    {
        var changes = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.TenantId == tenantId && ConfigEntities.Contains(a.EntityName))
            .OrderByDescending(a => a.Timestamp).Take(5)
            .Select(a => new ConfigChangeSummary { Id = a.Id, Timestamp = a.Timestamp, EntityName = a.EntityName, Action = a.Action, UserId = a.UserId })
            .ToListAsync(ct);
        var userIds = changes.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).Distinct().ToList();
        var names = await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FullName ?? u.PrimaryEmail ?? u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);
        foreach (var change in changes)
            change.UserName = change.UserId is Guid id ? names.GetValueOrDefault(id) : null;
        return changes;
    }

    private async Task<List<SetupChecklistItem>> SetupAsync(Guid tenantId, Guid? companyId, CancellationToken ct)
    {
        var company = companyId is Guid id
            ? await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, ct)
            : null;
        var settings = await _db.SystemSettings.AsNoTracking().Where(s => s.TenantId == tenantId)
            .Select(s => new { s.SettingKey, s.SettingValue }).ToListAsync(ct);
        bool Has(string key) => settings.Any(s => s.SettingKey == key && !string.IsNullOrEmpty(s.SettingValue));
        var sequencesReviewed = await _db.DocumentSequences.AsNoTracking().AnyAsync(s => s.TenantId == tenantId && s.UpdatedAt != null, ct);

        return
        [
            new() { Key = "company", Label = "Complete the company profile (legal name, address, country)", Link = "/core/settings/company",
                Done = company != null && !string.IsNullOrWhiteSpace(company.LegalName) && !string.IsNullOrWhiteSpace(company.Country)
                       && !string.IsNullOrWhiteSpace(company.AddressLine1 ?? company.Address) },
            new() { Key = "regional", Label = "Choose currency, time zone and language", Link = "/core/settings/preferences",
                Done = company != null && !string.IsNullOrWhiteSpace(company.Currency) && company.TimeZone is { Length: > 0 } tz && tz != "UTC" && Has("preferences.locale") },
            new() { Key = "numbering", Label = "Review document numbering", Link = "/core/settings/numbering", Done = sequencesReviewed },
            new() { Key = "email", Label = "Configure outgoing email", Link = "/core/settings/email",
                Done = Has("email.smtpHost") && Has("email.fromAddress") },
        ];
    }

    private async Task<List<TenantStatusCount>> TenantsAsync(CancellationToken ct) =>
        await _db.Tenants.AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new TenantStatusCount { Status = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync(ct);
}
