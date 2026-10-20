namespace ERP.Application.Core;

public interface ITenantEmailSender
{
    /// <summary>Sends email using tenant SMTP settings when configured; otherwise no-ops (caller may log).</summary>
    Task<bool> TrySendAsync(Guid tenantId, string toEmail, string subject, string bodyText, CancellationToken ct = default);

    /// <summary>Sends email with optional binary attachment. Returns false when SMTP is not configured.</summary>
    Task<bool> TrySendWithAttachmentAsync(
        Guid tenantId,
        string toEmail,
        string subject,
        string bodyText,
        byte[]? attachmentContent,
        string? attachmentFileName,
        string? attachmentContentType,
        CancellationToken ct = default);
}
