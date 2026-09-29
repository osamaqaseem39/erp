using System.Text.Json;
using ERP.Domain.Admin;
using ERP.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Logging;

public static class SystemLogSources
{
    public const string Security = "Security";
    public const string Api = "Api";
}

public interface ISystemLogWriter
{
    /// <summary>
    /// Appends to the tenant's system log. Never throws: a logging failure must not replace the
    /// error or response it was describing.
    /// </summary>
    Task WriteAsync(string level, string source, string message, Guid tenantId, Guid? userId = null,
        string? exception = null, string? method = null, string? ipAddress = null, object? properties = null);
}

/// <summary>Writes through its own DbContext scope, so a failed request's context can't block the entry.</summary>
public sealed class SystemLogWriter : ISystemLogWriter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<SystemLogWriter> _logger;

    public SystemLogWriter(IServiceScopeFactory scopes, ILogger<SystemLogWriter> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    public async Task WriteAsync(string level, string source, string message, Guid tenantId, Guid? userId = null,
        string? exception = null, string? method = null, string? ipAddress = null, object? properties = null)
    {
        if (tenantId == Guid.Empty) return;
        try
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
            db.SystemLogs.Add(new SystemLog
            {
                TenantId = tenantId,
                LogLevel = level,
                Source = source,
                Message = message.Length > 2000 ? message[..2000] : message,
                Exception = exception,
                Method = method,
                UserId = userId,
                IPAddress = ipAddress,
                Properties = properties == null ? null : JsonSerializer.Serialize(properties, Json),
                Timestamp = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not write system log entry: {Message}", message);
        }
    }
}
