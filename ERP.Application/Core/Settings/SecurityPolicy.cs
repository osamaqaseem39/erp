using System.Globalization;
using ERP.Infrastructure.Database;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Core.Settings;

/// <summary>A tenant's Security settings (Settings › Security), as enforced by sign-in and password changes.</summary>
public sealed record SecurityPolicy(int SessionTimeoutMinutes, int PasswordMinLength, int LockoutThreshold, int LockoutMinutes)
{
    public const int PasswordMaxLength = 128;

    /// <summary>Throws a field-level validation error when the password breaks the policy.</summary>
    public void EnsurePasswordAllowed(string? password, string field = "newPassword")
    {
        if (string.IsNullOrEmpty(password) || password.Length < PasswordMinLength)
            throw ValidationException.ForField(field, $"Password must be at least {PasswordMinLength} characters.");
        if (password.Length > PasswordMaxLength)
            throw ValidationException.ForField(field, $"Password can't be longer than {PasswordMaxLength} characters.");
    }
}

public interface ISecurityPolicyProvider
{
    /// <summary>
    /// Reads the policy for an explicit tenant. Sign-in runs before there is a tenant in context,
    /// so this doesn't use the request's tenant.
    /// </summary>
    Task<SecurityPolicy> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed class SecurityPolicyProvider : ISecurityPolicyProvider
{
    private static readonly string[] Keys =
        ["security.sessionTimeoutMinutes", "security.passwordMinLength", "security.lockoutThreshold", "security.lockoutMinutes"];

    private readonly ERPDbContext _db;

    public SecurityPolicyProvider(ERPDbContext db)
    {
        _db = db;
    }

    public async Task<SecurityPolicy> GetAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var stored = await _db.SystemSettings.AsNoTracking()
            .Where(s => s.TenantId == tenantId && Keys.Contains(s.SettingKey))
            .Select(s => new { s.SettingKey, s.SettingValue })
            .ToListAsync(cancellationToken);
        var values = stored.GroupBy(s => s.SettingKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().SettingValue, StringComparer.OrdinalIgnoreCase);

        return new SecurityPolicy(
            Read(values, Keys[0]),
            Read(values, Keys[1]),
            Read(values, Keys[2]),
            Read(values, Keys[3]));
    }

    /// <summary>The stored value when valid, else the catalog default; always within the catalog's limits.</summary>
    private static int Read(Dictionary<string, string?> values, string key)
    {
        var definition = SettingsCatalog.Find(key)!;
        var fallback = int.Parse(definition.Default!, CultureInfo.InvariantCulture);
        var value = values.TryGetValue(key, out var raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
        return (int)Math.Clamp(value, definition.Min ?? value, definition.Max ?? value);
    }
}
