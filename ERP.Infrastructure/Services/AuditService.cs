using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace ERP.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IStoredProcedureExecutor _executor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IStoredProcedureExecutor executor, IHttpContextAccessor httpContextAccessor)
    {
        _executor = executor;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var tenantId = GetTenantId(context);
        var userId = GetUserId(context);
        
        if (tenantId == null || userId == null) return;

        ipAddress ??= GetClientIpAddress(context);
        userAgent ??= context.Request.Headers["User-Agent"].ToString();

        var auditLog = new AuditLog
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
        };

        await _executor.ExecuteScalarAsync<Guid>("addAuditLog", new
        {
            Id = auditLog.Id,
            TenantId = auditLog.TenantId,
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            EntityName = auditLog.EntityName,
            EntityId = auditLog.EntityId,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            Timestamp = auditLog.Timestamp,
            CreatedAt = auditLog.CreatedAt,
            CreatedBy = auditLog.CreatedBy
        });
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 50)
    {
        var logs = await _executor.ExecuteQueryAsync<AuditLogDto>("getAuditLogsByTenant", new
        {
            TenantId = tenantId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        return logs;
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id)
    {
        return await _executor.ExecuteSingleAsync<AuditLogDto>("getAuditLogById", new { Id = id });
    }

    private static Guid? GetTenantId(HttpContext context)
    {
        // Try to get from JWT claims
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = user.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return tenantId;
            }
        }

        // Try to get from header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (Guid.TryParse(tenantIdHeader, out var headerTenantId))
            {
                return headerTenantId;
            }
        }

        return null;
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }

        return null;
    }

    private static string? GetUserEmail(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        return null;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        }

        return ipAddress ?? "Unknown";
    }
}