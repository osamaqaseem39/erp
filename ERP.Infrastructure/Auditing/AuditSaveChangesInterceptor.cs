using System.Text.Json;
using ERP.Domain;
using ERP.Domain.Admin;
using ERP.Domain.Core;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Infrastructure.Auditing;

/// <summary>
/// Writes an AuditLog row for every create, update and delete of tenant data, in the same transaction
/// as the change. Stores only what changed (before/after), redacts secrets, and skips bookkeeping-only
/// updates such as a user's last sign-in time.
///
/// Changes made without a signed-in user (first-run setup, background work) are not audited:
/// AuditLog.UserId is a required foreign key and a made-up user would be worse than no entry.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public const int MaxValuesLength = 4000;
    public const string Redacted = "[redacted]";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Tables that are themselves logs or high-volume session state.</summary>
    private static readonly HashSet<Type> ExcludedTypes = [typeof(AuditLog), typeof(SystemLog), typeof(UserSession), typeof(RefreshToken)];

    /// <summary>Never written to the audit trail in clear text.</summary>
    private static readonly HashSet<string> SecretProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "PasswordSalt", "RefreshToken", "RefreshTokenExpires", "EmailVerificationToken",
        "PasswordResetToken", "PasswordResetExpires", "SessionToken", "ConnectionString", "ApiKey", "Secret",
    };

    /// <summary>Changes to only these properties are bookkeeping, not business changes.</summary>
    private static readonly HashSet<string> NoiseProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "UpdatedAt", "UpdatedBy", "CreatedAt", "CreatedBy", "LastLoginAt", "LastLoginDate", "FailedLoginAttempts",
        "LockedUntil", "RefreshToken", "RefreshTokenExpires",
    };

    private readonly IAuditContext _audit;

    public AuditSaveChangesInterceptor(IAuditContext audit)
    {
        _audit = audit;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context is null || _audit.UserId is not Guid userId || userId == Guid.Empty) return;

        context.ChangeTracker.DetectChanges();
        var logs = new List<AuditLog>();
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>().ToList())
        {
            if (ExcludedTypes.Contains(entry.Entity.GetType())) continue;
            var log = BuildLog(entry, userId);
            if (log != null) logs.Add(log);
        }
        if (logs.Count > 0) context.Set<AuditLog>().AddRange(logs);
    }

    private AuditLog? BuildLog(EntityEntry<BaseEntity> entry, Guid userId)
    {
        string action;
        Dictionary<string, object?>? before = null;
        Dictionary<string, object?>? after = null;

        switch (entry.State)
        {
            case EntityState.Added:
                action = "Created";
                after = entry.Properties
                    .Where(p => p.CurrentValue != null && !NoiseProperties.Contains(p.Metadata.Name) && p.Metadata.Name != "Id" && p.Metadata.Name != "TenantId")
                    .ToDictionary(p => p.Metadata.Name, p => Value(entry, p.Metadata.Name, p.CurrentValue));
                break;
            case EntityState.Deleted:
                action = "Deleted";
                before = entry.Properties
                    .Where(p => p.OriginalValue != null && !NoiseProperties.Contains(p.Metadata.Name) && p.Metadata.Name != "Id" && p.Metadata.Name != "TenantId")
                    .ToDictionary(p => p.Metadata.Name, p => Value(entry, p.Metadata.Name, p.OriginalValue));
                break;
            case EntityState.Modified:
                var changed = entry.Properties
                    .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue) && !NoiseProperties.Contains(p.Metadata.Name))
                    .ToList();
                if (changed.Count == 0) return null;
                action = "Updated";
                before = changed.ToDictionary(p => p.Metadata.Name, p => Value(entry, p.Metadata.Name, p.OriginalValue));
                after = changed.ToDictionary(p => p.Metadata.Name, p => Value(entry, p.Metadata.Name, p.CurrentValue));
                break;
            default:
                return null;
        }

        var tenantId = entry.Entity.TenantId != Guid.Empty ? entry.Entity.TenantId : _audit.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty) return null;

        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            Action = action,
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            OldValues = Serialize(before),
            NewValues = Serialize(after),
            IpAddress = Truncate(_audit.IpAddress, 50),
            UserAgent = Truncate(_audit.UserAgent, 500),
            Timestamp = DateTime.UtcNow,
        };
    }

    private static object? Value(EntityEntry<BaseEntity> entry, string property, object? value)
    {
        if (value is null) return null;
        if (SecretProperties.Contains(property)) return Redacted;
        // Encrypted settings: the value itself is a secret even though the column is generic.
        if (entry.Entity is SystemSetting { IsEncrypted: true } && property == nameof(SystemSetting.SettingValue)) return Redacted;
        return value;
    }

    private static string Serialize(Dictionary<string, object?>? values)
    {
        if (values is null || values.Count == 0) return string.Empty;
        var json = JsonSerializer.Serialize(values, Json);
        return json.Length <= MaxValuesLength ? json : JsonSerializer.Serialize(new { truncated = true, fields = values.Keys }, Json);
    }

    private static string Truncate(string? value, int max) => string.IsNullOrEmpty(value) ? string.Empty : value.Length <= max ? value : value[..max];
}
