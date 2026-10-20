using System.Net;
using System.Net.Mail;
using ERP.Application.Core.Settings;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Core;

/// <summary>Sends mail via tenant <c>email.*</c> settings. Returns false when SMTP is not configured.</summary>
public sealed class SmtpTenantEmailSender : ITenantEmailSender
{
    private readonly ERPDbContext _db;
    private readonly ILogger<SmtpTenantEmailSender> _logger;

    public SmtpTenantEmailSender(ERPDbContext db, ILogger<SmtpTenantEmailSender> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> TrySendAsync(Guid tenantId, string toEmail, string subject, string bodyText, CancellationToken ct = default)
    {
        var keys = new[]
        {
            "email.smtpHost", "email.smtpPort", "email.useSsl", "email.smtpUser", "email.smtpPassword",
            "email.fromAddress", "email.fromName"
        };
        var stored = await _db.SystemSettings.AsNoTracking()
            .Where(s => s.TenantId == tenantId && keys.Contains(s.SettingKey))
            .ToDictionaryAsync(s => s.SettingKey, s => (string?)s.SettingValue, StringComparer.OrdinalIgnoreCase, ct);

        var host = Get(stored, "email.smtpHost");
        var from = Get(stored, "email.fromAddress");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogInformation("SMTP not configured for tenant {TenantId}; email to {To} skipped.", tenantId, toEmail);
            return false;
        }

        var port = int.TryParse(Get(stored, "email.smtpPort"), out var p) ? p : 587;
        var useSsl = !string.Equals(Get(stored, "email.useSsl"), "false", StringComparison.OrdinalIgnoreCase);
        var user = Get(stored, "email.smtpUser");
        var password = Get(stored, "email.smtpPassword");
        var fromName = Get(stored, "email.fromName") ?? "ERP";

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = bodyText,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        if (!string.IsNullOrWhiteSpace(user))
            client.Credentials = new NetworkCredential(user, password);

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email sent to {To} for tenant {TenantId}.", toEmail, tenantId);
        return true;
    }

    private static string? Get(IReadOnlyDictionary<string, string?> map, string key) =>
        map.TryGetValue(key, out var v) ? v : null;
}
