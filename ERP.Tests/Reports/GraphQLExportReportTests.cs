using ERP.Api.GraphQL.Reports;
using ERP.Application.Finance;
using ERP.Application.Reports;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class GraphQLExportReportTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

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

    [Fact]
    public void Mutations_ExposeExportReport()
    {
        var names = typeof(ReportsMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ReportsMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("ExportReport", names);
    }

    [Fact]
    public async Task GraphQL_ExportReport_Json_And_RejectsPdf()
    {
        await using var db = Db();
        var svc = Reports(db);
        await svc.GetAvailableReportsAsync(Tenant);
        var report = await db.Reports.FirstAsync(r =>
            r.TenantId == Tenant && r.ReportCode == "SALE_INVOICE_SUMMARY");

        var gql = new ReportsMutations();
        var json = await gql.ExportReport(svc, report.Id, Tenant, "json");
        Assert.Equal("application/json", json.ContentType);
        Assert.True(json.Content.Length > 2);

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            gql.ExportReport(svc, report.Id, Tenant, "pdf"));
    }
}
