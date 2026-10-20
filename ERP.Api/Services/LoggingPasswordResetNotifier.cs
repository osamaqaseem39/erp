using ERP.Application.Core.Auth;

namespace ERP.Api.Services;

/// <summary>Logs reset-token issuance so local/dev works without SMTP. Replace with real email later.</summary>
public sealed class LoggingPasswordResetNotifier : IPasswordResetNotifier
{
    private readonly ILogger<LoggingPasswordResetNotifier> _logger;

    public LoggingPasswordResetNotifier(ILogger<LoggingPasswordResetNotifier> logger) => _logger = logger;

    public Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        // Never log the raw token in production hosts — Development/Testing only.
        _logger.LogInformation(
            "Password reset issued for {Email} (tenant {TenantId}); expires {ExpiresAtUtc:o}.",
            email, tenantId, expiresAtUtc);
#if DEBUG
        _logger.LogDebug("Password reset raw token (DEBUG only): {Token}", rawToken);
#endif
        return Task.CompletedTask;
    }
}
