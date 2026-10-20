using ERP.Application.Core;
using ERP.Application.Core.Auth;

namespace ERP.Api.Services;

/// <summary>Tries SMTP from tenant settings; always logs issuance for ops visibility.</summary>
public sealed class SmtpPasswordResetNotifier : IPasswordResetNotifier
{
    private readonly ITenantEmailSender _email;
    private readonly ILogger<SmtpPasswordResetNotifier> _logger;

    public SmtpPasswordResetNotifier(ITenantEmailSender email, ILogger<SmtpPasswordResetNotifier> logger)
    {
        _email = email;
        _logger = logger;
    }

    public async Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Password reset issued for {Email} (tenant {TenantId}); expires {ExpiresAtUtc:o}.",
            email, tenantId, expiresAtUtc);

        var body =
            $"A password reset was requested for your ERP account.\n\n" +
            $"Reset token: {rawToken}\n" +
            $"Expires (UTC): {expiresAtUtc:o}\n\n" +
            "If you did not request this, ignore this message.";

        var sent = await _email.TrySendAsync(tenantId, email, "Password reset", body, ct);
        if (!sent)
            _logger.LogWarning("Password reset email not sent (SMTP not configured) for {Email}.", email);
#if DEBUG
        _logger.LogDebug("Password reset raw token (DEBUG only): {Token}", rawToken);
#endif
    }
}
