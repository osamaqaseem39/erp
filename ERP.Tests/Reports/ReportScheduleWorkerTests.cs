using ERP.Application.Finance;
using ERP.Application.Reports;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class ReportScheduleWorkerTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ReportService Reports(ERPDbContext db)
    {
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        return new ReportService(
            new ReportRepository(db),
            new ReportExecutionRepository(db),
            new ReportScheduleRepository(db),
            accounting,
            invoices,
            db);
    }

    [Theory]
    [InlineData("Daily", 1)]
    [InlineData("Weekly", 7)]
    [InlineData("Hourly", 0)]
    public void Calculator_Advances_By_ScheduleType(string type, int dayDelta)
    {
        var from = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
        var next = ReportScheduleCalculator.ComputeNextRun(type, from);
        if (type == "Hourly")
            Assert.Equal(from.AddHours(1), next);
        else
            Assert.Equal(from.AddDays(dayDelta), next);
    }

    [Fact]
    public async Task Schedule_Sets_NextRun_And_ExecuteDue_Writes_History()
    {
        await using var db = Db();
        var svc = Reports(db);
        await svc.GetAvailableReportsAsync(Tenant);
        var report = await db.Reports.FirstAsync(r =>
            r.TenantId == Tenant && r.ReportCode == "SALE_INVOICE_SUMMARY");

        var scheduled = await svc.ScheduleReportAsync(new ScheduleReportDto
        {
            ReportId = report.Id,
            ScheduleType = "Daily",
            ScheduleExpression = "0 8 * * *",
            Email = "ops@example.com"
        }, Tenant, User);

        Assert.True(scheduled.IsActive);
        Assert.NotNull(scheduled.NextRunTime);
        Assert.True(scheduled.NextRunTime <= DateTime.UtcNow.AddMinutes(1));
        Assert.Contains("Daily", scheduled.ScheduleName, StringComparison.OrdinalIgnoreCase);

        var row = await db.ReportSchedules.SingleAsync(s => s.Id == scheduled.Id);
        row.NextRunTime = DateTime.UtcNow.AddMinutes(-5);
        await db.SaveChangesAsync();

        var ran = await svc.ExecuteDueSchedulesAsync();
        Assert.Equal(1, ran);

        var executions = await db.ReportExecutions
            .Where(e => e.TenantId == Tenant && e.ScheduleId == scheduled.Id)
            .ToListAsync();
        Assert.Single(executions);
        Assert.Equal("Completed", executions[0].Status);

        var updated = await db.ReportSchedules.SingleAsync(s => s.Id == scheduled.Id);
        Assert.NotNull(updated.LastRunTime);
        Assert.True(updated.NextRunTime > DateTime.UtcNow);
        Assert.Equal("Daily", updated.ScheduleType);

        Assert.Equal(0, await svc.ExecuteDueSchedulesAsync());
    }

    [Fact]
    public async Task Cancelled_Schedule_Is_Not_Executed()
    {
        await using var db = Db();
        var svc = Reports(db);
        await svc.GetAvailableReportsAsync(Tenant);
        var report = await db.Reports.FirstAsync(r => r.TenantId == Tenant);

        var scheduled = await svc.ScheduleReportAsync(new ScheduleReportDto
        {
            ReportId = report.Id,
            ScheduleType = "Hourly"
        }, Tenant, User);

        Assert.True(await svc.CancelScheduledReportAsync(scheduled.Id!.Value, Tenant));

        var row = await db.ReportSchedules.SingleAsync(s => s.Id == scheduled.Id);
        row.NextRunTime = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        Assert.Equal(0, await svc.ExecuteDueSchedulesAsync());
        Assert.Empty(await db.ReportExecutions.Where(e => e.ScheduleId == scheduled.Id).ToListAsync());
    }

    [Fact]
    public async Task Export_Json_And_Csv_Honest_Mime_Rejects_Pdf()
    {
        await using var db = Db();
        var svc = Reports(db);
        await svc.GetAvailableReportsAsync(Tenant);
        var report = await db.Reports.FirstAsync(r =>
            r.TenantId == Tenant && r.ReportCode == "SALE_INVOICE_SUMMARY");

        var json = await svc.ExportReportAsync(report.Id, Tenant, "json");
        Assert.Equal("application/json", json.ContentType);
        Assert.EndsWith(".json", json.FileName);
        Assert.True(json.Content.Length > 2);

        var csv = await svc.ExportReportAsync(report.Id, Tenant, "csv");
        Assert.Equal("text/csv", csv.ContentType);
        Assert.EndsWith(".csv", csv.FileName);

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            svc.ExportReportAsync(report.Id, Tenant, "pdf"));
    }
}
