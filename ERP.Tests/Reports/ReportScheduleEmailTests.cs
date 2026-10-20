using ERP.Application.Core;
using ERP.Application.Finance;
using ERP.Application.Reports;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class ReportScheduleEmailTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private sealed class FakeEmail : ITenantEmailSender
    {
        public List<(Guid TenantId, string To, string Subject, string? FileName)> Sent { get; } = [];

        public Task<bool> TrySendAsync(Guid tenantId, string toEmail, string subject, string bodyText, CancellationToken ct = default)
        {
            Sent.Add((tenantId, toEmail, subject, null));
            return Task.FromResult(true);
        }

        public Task<bool> TrySendWithAttachmentAsync(
            Guid tenantId, string toEmail, string subject, string bodyText,
            byte[]? attachmentContent, string? attachmentFileName, string? attachmentContentType,
            CancellationToken ct = default)
        {
            Sent.Add((tenantId, toEmail, subject, attachmentFileName));
            return Task.FromResult(true);
        }
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task ExecuteDue_WithRecipients_SendsEmailAttachment()
    {
        await using var db = Db();
        var email = new FakeEmail();
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var svc = new ReportService(
            new ReportRepository(db),
            new ReportExecutionRepository(db),
            new ReportScheduleRepository(db),
            accounting,
            invoices,
            db,
            email: email);

        await svc.GetAvailableReportsAsync(Tenant);
        var report = await db.Reports.FirstAsync(r =>
            r.TenantId == Tenant && r.ReportCode == "SALE_INVOICE_SUMMARY");

        var scheduled = await svc.ScheduleReportAsync(new ScheduleReportDto
        {
            ReportId = report.Id,
            ScheduleType = "Daily",
            ScheduleExpression = "0 8 * * *",
            Email = "ops@example.com;finance@example.com"
        }, Tenant, User);

        var row = await db.ReportSchedules.SingleAsync(s => s.Id == scheduled.Id);
        row.NextRunTime = DateTime.UtcNow.AddMinutes(-5);
        await db.SaveChangesAsync();

        var ran = await svc.ExecuteDueSchedulesAsync();
        Assert.Equal(1, ran);
        Assert.Equal(2, email.Sent.Count);
        Assert.All(email.Sent, s => Assert.Equal(Tenant, s.TenantId));
        Assert.Contains(email.Sent, s => s.To == "ops@example.com" && s.FileName != null);
        Assert.Contains(email.Sent, s => s.To == "finance@example.com");
    }
}
