namespace ERP.Application.Core;

public interface ITenantEmailSender
{
    /// <summary>Sends email using tenant SMTP settings when configured; otherwise no-ops (caller may log).</summary>
    Task<bool> TrySendAsync(Guid tenantId, string toEmail, string subject, string bodyText, CancellationToken ct = default);
}
