using System.Text.Json;
using ERP.Application.Finance;
using ERP.Application.Reports;
using ERP.Domain.CRM;
using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class SalesCatalogReportTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

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
    public async Task Available_Seeds_Catalog_And_Execute_Sale_Summaries()
    {
        await using var db = Db();
        db.Customers.Add(new Customer
        {
            Id = Customer,
            TenantId = Tenant,
            CustomerCode = "C1",
            CustomerName = "Acme",
            CreditDays = 30,
            IsActive = true
        });
        db.SalesInvoices.Add(new SalesInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            InvoiceNumber = "SI-1",
            CustomerId = Customer,
            InvoiceDate = new DateTime(2026, 10, 1),
            DueDate = new DateTime(2026, 10, 31),
            Status = "Posted",
            SubTotal = 100,
            TaxAmount = 0,
            TotalAmount = 100,
            PaidAmount = 40,
            BalanceAmount = 60
        });
        var inv2 = Guid.NewGuid();
        db.SalesInvoices.Add(new SalesInvoice
        {
            Id = inv2,
            TenantId = Tenant,
            InvoiceNumber = "SI-2",
            CustomerId = Customer,
            InvoiceDate = new DateTime(2026, 10, 5),
            DueDate = new DateTime(2026, 11, 5),
            Status = "Posted",
            SubTotal = 50,
            TotalAmount = 50,
            BalanceAmount = 50
        });
        db.SalesInvoiceLines.Add(new SalesInvoiceLine
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            SalesInvoiceId = inv2,
            ItemId = Item,
            Description = "Widget",
            Quantity = 2,
            UnitPrice = 25,
            LineTotal = 50,
            LineNumber = 1
        });
        db.SalesOrders.Add(new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            OrderNumber = "SO-OPEN",
            SalesOrderNumber = "SO-OPEN",
            CustomerId = Customer,
            OrderDate = new DateTime(2026, 10, 2),
            Status = "Confirmed",
            TotalAmount = 75
        });
        await db.SaveChangesAsync();

        var svc = Reports(db);
        var available = (await svc.GetAvailableReportsAsync(Tenant)).ToList();
        Assert.Contains(available, r => r.ReportCode == "SALE_INVOICE_SUMMARY");
        Assert.Contains(available, r => r.ReportCode == "PENDING_SALE_ORDER");
        Assert.Contains(available, r => r.ReportCode == "SALE_SUMMARY_ITEM");

        var summaryReport = available.First(r => r.ReportCode == "SALE_INVOICE_SUMMARY");
        var exec = await svc.GenerateReportAsync(new GenerateReportDto
        {
            ReportId = summaryReport.Id!.Value,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        }, Tenant, User);

        Assert.Equal("Completed", exec.Status);
        Assert.False(string.IsNullOrWhiteSpace(exec.ResultData));
        using var doc = JsonDocument.Parse(exec.ResultData!);
        Assert.Equal(2, doc.RootElement.GetProperty("InvoiceCount").GetInt32());
        Assert.Equal(150m, doc.RootElement.GetProperty("TotalAmount").GetDecimal());

        var pending = await svc.GenerateReportAsync(new GenerateReportDto
        {
            ReportType = "PENDING_SALE_ORDER"
        }, Tenant, User);
        Assert.Equal("Completed", pending.Status);
        using var pendingDoc = JsonDocument.Parse(pending.ResultData!);
        Assert.Equal(1, pendingDoc.RootElement.GetProperty("TotalCount").GetInt32());

        var byItem = await svc.GenerateReportAsync(new GenerateReportDto
        {
            ReportType = "SALE_SUMMARY_ITEM",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        }, Tenant, User);
        using var itemDoc = JsonDocument.Parse(byItem.ResultData!);
        Assert.Equal(1, itemDoc.RootElement.GetProperty("TotalCount").GetInt32());
        Assert.Equal(50m, itemDoc.RootElement.GetProperty("TotalAmount").GetDecimal());
    }
}
