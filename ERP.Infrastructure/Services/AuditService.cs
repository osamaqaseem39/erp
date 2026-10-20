using System.Security.Claims;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Explicit audit logging and reads via EF (same AuditLogs store as AuditSaveChangesInterceptor).
/// </summary>
public class AuditService : IAuditService
{
    private readonly ERPDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ERPDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string entityId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var tenantId = GetTenantId(context);
        var userId = GetUserId(context);
        if (tenantId == null || userId == null) return;

        ipAddress ??= GetClientIpAddress(context);
        userAgent ??= context.Request.Headers["User-Agent"].ToString();

        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues ?? string.Empty,
            NewValues = newValues ?? string.Empty,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetUserEmail(context) ?? "System"
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 50)
    {
        if (tenantId == Guid.Empty) return Array.Empty<AuditLogDto>();
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var rows = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return rows.Select(Map);
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id)
    {
        if (id == Guid.Empty) return null;

        // SuperAdmin GraphQL may pass any id; when a tenant is present, keep reads fail-closed.
        var context = _httpContextAccessor.HttpContext;
        var tenantId = context != null ? GetTenantId(context) : null;

        var q = _db.AuditLogs.AsNoTracking().Where(a => a.Id == id);
        if (tenantId is Guid tid && tid != Guid.Empty)
            q = q.Where(a => a.TenantId == tid);

        var row = await q.FirstOrDefaultAsync();
        return row == null ? null : Map(row);
    }

    private static AuditLogDto Map(AuditLog a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        UserId = a.UserId,
        Action = a.Action,
        EntityName = a.EntityName,
        EntityId = a.EntityId,
        OldValues = a.OldValues,
        NewValues = a.NewValues,
        IpAddress = a.IpAddress,
        UserAgent = a.UserAgent,
        Timestamp = a.Timestamp,
        CreatedAt = a.CreatedAt,
        CreatedBy = a.CreatedBy,
        IsActive = a.IsActive
    };

    private static Guid? GetTenantId(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = user.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                return tenantId;
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader)
            && Guid.TryParse(tenantIdHeader, out var headerTenantId))
            return headerTenantId;

        return null;
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) return null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    private static string? GetUserEmail(HttpContext context) =>
        context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(ClaimTypes.Email)?.Value
            : null;

    private static string GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        return ipAddress ?? "Unknown";
    }
}
