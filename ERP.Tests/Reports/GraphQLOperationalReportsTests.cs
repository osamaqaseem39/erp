using ERP.Api.GraphQL.Reports;
using ERP.Application.Reports;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class GraphQLOperationalReportsTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesOperationalReports()
    {
        var queries = typeof(ReportsQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(ReportsQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetPendingSalesOrdersReport", queries);
        Assert.Contains("GetSalesInvoiceSummaryReport", queries);
        Assert.Contains("GetSalesSummaryByItemReport", queries);
        Assert.Contains("GetSalesSummaryCustomerItemReport", queries);
        Assert.Contains("GetSalesSummaryItemCustomerReport", queries);
        Assert.Contains("GetSalesItemRateDetailReport", queries);
        Assert.Contains("GetCustomerInformationReport", queries);
        Assert.Contains("GetCreditNoteReport", queries);
        Assert.Contains("GetPurchaseInvoiceSummaryReport", queries);
        Assert.Contains("GetStockBalanceReport", queries);
        Assert.Contains("GetSaleAgingReport", queries);
    }

    [Fact]
    public async Task GraphQL_PendingSalesOrders_EmptyTenant()
    {
        await using var db = Db();
        var queries = new ReportsQueries();
        var service = new OperationalReportService(db);

        var report = await queries.GetPendingSalesOrdersReport(service, Tenant);
        Assert.Equal(0, report.OrderCount);
        Assert.Empty(report.Lines);
    }
}
