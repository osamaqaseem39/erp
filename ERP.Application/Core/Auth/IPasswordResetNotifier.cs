using Microsoft.Extensions.Logging;

namespace ERP.Application.Core.Auth;

/// <summary>
/// Delivers the one-time password-reset token. Production wires SMTP/provider here;
/// tests use <see cref="CapturingPasswordResetNotifier"/>.
/// </summary>
public interface IPasswordResetNotifier
{
    Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default);
}

/// <summary>No-op delivery — AuthService still stores the hashed token.</summary>
public sealed class NullPasswordResetNotifier : IPasswordResetNotifier
{
    public Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default)
        => Task.CompletedTask;
}

/// <summary>Default host registration: log that a token was issued (no SMTP yet).</summary>
public sealed class LoggingPasswordResetNotifier : IPasswordResetNotifier
{
    private readonly ILogger<LoggingPasswordResetNotifier> _logger;

    public LoggingPasswordResetNotifier(ILogger<LoggingPasswordResetNotifier> logger) => _logger = logger;

    public Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Password reset token issued for {Email} (tenant {TenantId}); expires {ExpiresAt:o}. Wire SMTP for delivery.",
            email, tenantId, expiresAtUtc);
        return Task.CompletedTask;
    }
}

/// <summary>Test double that retains the last issued raw token.</summary>
public sealed class CapturingPasswordResetNotifier : IPasswordResetNotifier
{
    public string? LastEmail { get; private set; }
    public Guid LastTenantId { get; private set; }
    public string? LastToken { get; private set; }
    public DateTime? LastExpiresAt { get; private set; }

    public void Clear()
    {
        LastEmail = null;
        LastTenantId = default;
        LastToken = null;
        LastExpiresAt = null;
    }

    public Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        LastEmail = email;
        LastTenantId = tenantId;
        LastToken = rawToken;
        LastExpiresAt = expiresAtUtc;
        return Task.CompletedTask;
    }
}
