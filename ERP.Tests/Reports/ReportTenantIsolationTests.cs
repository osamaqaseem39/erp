using ERP.Application.Finance;
using ERP.Application.Reports;
using ERP.Domain.Reports;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class ReportTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ReportService Svc(ERPDbContext db)
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

    [Fact]
    public async Task GetAll_Only_Callers_Tenant()
    {
        await using var db = Db();
        db.Reports.AddRange(
            new Report { Id = Guid.NewGuid(), TenantId = TenantA, ReportCode = "A", Name = "A", ReportName = "A", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Report { Id = Guid.NewGuid(), TenantId = TenantB, ReportCode = "B", Name = "B", ReportName = "B", IsActive = true, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var all = (await Svc(db).GetAllAsync(TenantA)).ToList();
        Assert.Contains(all, r => r.ReportCode == "A");
        Assert.DoesNotContain(all, r => r.ReportCode == "B");
        Assert.All(all, r => Assert.Equal(TenantA, r.TenantId));
    }

    [Fact]
    public async Task Other_Tenant_Cannot_Get_Update_Delete_Or_Execute()
    {
        await using var db = Db();
        var reportId = Guid.NewGuid();
        db.Reports.Add(new Report
        {
            Id = reportId,
            TenantId = TenantB,
            ReportCode = "SECRET",
            Name = "Secret",
            ReportName = "Secret",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        Assert.Null(await svc.GetByIdAsync(reportId, TenantA));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateAsync(new UpdateReportDto
            {
                Id = reportId,
                Name = "Hijack",
                IsActive = true
            }, TenantA, UserA));
        Assert.False(await svc.DeleteAsync(reportId, TenantA));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.ExecuteAsync(reportId, null, TenantA, UserA));

        Assert.Equal("Secret", (await db.Reports.SingleAsync(r => r.Id == reportId)).Name);
    }

    [Fact]
    public async Task Cancel_Schedule_Rejects_Other_Tenant()
    {
        await using var db = Db();
        var reportId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();
        db.Reports.Add(new Report
        {
            Id = reportId,
            TenantId = TenantB,
            ReportCode = "X",
            Name = "X",
            ReportName = "X",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        db.ReportSchedules.Add(new ReportSchedule
        {
            Id = scheduleId,
            TenantId = TenantB,
            ReportId = reportId,
            ScheduleType = "Daily",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        Assert.False(await Svc(db).CancelScheduledReportAsync(scheduleId, TenantA));
        Assert.True((await db.ReportSchedules.SingleAsync(s => s.Id == scheduleId)).IsActive);
    }

    [Fact]
    public async Task Empty_Tenant_Fails_Closed_On_Catalog()
    {
        await using var db = Db();
        db.Reports.Add(new Report
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            ReportCode = "A",
            Name = "A",
            ReportName = "A",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        Assert.Empty(await new ReportRepository(db).GetAllAsync(Guid.Empty));
        Assert.Null(await new ReportRepository(db).GetByIdAsync(db.Reports.First().Id, Guid.Empty));
    }

    [Fact]
    public async Task Execution_History_Is_Tenant_Scoped()
    {
        await using var db = Db();
        var reportA = Guid.NewGuid();
        var reportB = Guid.NewGuid();
        db.Reports.AddRange(
            new Report { Id = reportA, TenantId = TenantA, ReportCode = "A", Name = "A", ReportName = "A", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Report { Id = reportB, TenantId = TenantB, ReportCode = "B", Name = "B", ReportName = "B", IsActive = true, CreatedAt = DateTime.UtcNow });
        db.ReportExecutions.AddRange(
            new ReportExecution { Id = Guid.NewGuid(), TenantId = TenantA, ReportId = reportA, Status = "Completed", StartedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new ReportExecution { Id = Guid.NewGuid(), TenantId = TenantB, ReportId = reportB, Status = "Completed", StartedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var history = (await Svc(db).GetReportHistoryAsync(TenantA)).ToList();
        Assert.Single(history);
        Assert.Equal(reportA, history[0].ReportId);
    }
}
