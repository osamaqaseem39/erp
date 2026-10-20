using ERP.Application.Reports;
using ERP.Domain.CRM;
using ERP.Domain.Procurement;
using ERP.Domain.Sales;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Reports;

public class OperationalReportTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task PendingSalesOrders_Excludes_Invoiced_And_Cancelled()
    {
        await using var db = Db();
        db.Customers.Add(new Customer
        {
            Id = Customer, TenantId = Tenant, CustomerCode = "C1",
            CustomerName = "Acme", CompanyName = "Acme Ltd"
        });
        db.SalesOrders.AddRange(
            new SalesOrder
            {
                Id = Guid.NewGuid(), TenantId = Tenant, CustomerId = Customer,
                OrderNumber = "SO-OPEN", OrderDate = new DateTime(2026, 3, 1),
                Status = "Confirmed", TotalAmount = 100m
            },
            new SalesOrder
            {
                Id = Guid.NewGuid(), TenantId = Tenant, CustomerId = Customer,
                OrderNumber = "SO-DONE", OrderDate = new DateTime(2026, 3, 2),
                Status = "Invoiced", InvoicedDate = new DateTime(2026, 3, 5), TotalAmount = 50m
            },
            new SalesOrder
            {
                Id = Guid.NewGuid(), TenantId = Tenant, CustomerId = Customer,
                OrderNumber = "SO-CXL", OrderDate = new DateTime(2026, 3, 3),
                Status = "Cancelled", TotalAmount = 20m
            });
        await db.SaveChangesAsync();

        var report = await new OperationalReportService(db)
            .GetPendingSalesOrdersAsync(Tenant, new DateTime(2026, 3, 31));
        var line = Assert.Single(report.Lines);
        Assert.Equal("SO-OPEN", line.OrderNumber);
        Assert.Equal(100m, report.TotalValue);
        Assert.Equal("Acme Ltd", line.CustomerName);
    }

    [Fact]
    public async Task SalesInvoiceSummary_And_Item_Pivots()
    {
        await using var db = Db();
        db.Customers.Add(new Customer
        {
            Id = Customer, TenantId = Tenant, CustomerCode = "C1",
            CustomerName = "Acme", CompanyName = "Acme Ltd", CreditDays = 30, Ntn = "123"
        });
        var invId = Guid.NewGuid();
        db.SalesInvoices.Add(new SalesInvoice
        {
            Id = invId, TenantId = Tenant, CustomerId = Customer,
            InvoiceNumber = "SI-1", InvoiceDate = new DateTime(2026, 4, 10),
            DueDate = new DateTime(2026, 5, 10), Status = "Posted",
            SubTotal = 100m, TaxAmount = 12m, TotalAmount = 112m,
            PaidAmount = 0, BalanceAmount = 112m
        });
        db.SalesInvoiceLines.Add(new SalesInvoiceLine
        {
            Id = Guid.NewGuid(), TenantId = Tenant, SalesInvoiceId = invId,
            ItemId = Item, Description = "Widget", Quantity = 2, UnitPrice = 50,
            Tax1Rate = 10, Tax1Amount = 10, Tax2Rate = 2, Tax2Amount = 2,
            TaxRate = 10, TaxAmount = 12, LineTotal = 112, LineNumber = 1
        });
        await db.SaveChangesAsync();

        var svc = new OperationalReportService(db);
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 30);

        var summary = await svc.GetSalesInvoiceSummaryAsync(Tenant, from, to);
        Assert.Equal(1, summary.InvoiceCount);
        Assert.Equal(112m, summary.TotalAmount);

        var byItem = await svc.GetSalesSummaryByItemAsync(Tenant, from, to);
        Assert.Single(byItem.Lines);
        Assert.Equal(2m, byItem.Lines[0].Quantity);
        Assert.Equal(112m, byItem.TotalAmount);

        var cx = await svc.GetSalesSummaryCustomerItemAsync(Tenant, from, to);
        Assert.Single(cx.Lines);
        Assert.Equal(Customer, cx.Lines[0].CustomerId);
        Assert.Equal(Item, cx.Lines[0].ItemId);

        var rates = await svc.GetSalesItemRateDetailAsync(Tenant, from, to, Item);
        Assert.Single(rates.Lines);
        Assert.Equal(50m, rates.Lines[0].UnitPrice);
        Assert.Equal(10m, rates.Lines[0].Tax1Rate);
        Assert.Equal(2m, rates.Lines[0].Tax2Rate);

        var customers = await svc.GetCustomerInformationAsync(Tenant);
        Assert.Single(customers.Lines);
        Assert.Equal(30, customers.Lines[0].CreditDays);
        Assert.Equal("123", customers.Lines[0].Ntn);
    }

    [Fact]
    public async Task Purchase_And_Stock_Reports()
    {
        await using var db = Db();
        db.Vendors.Add(new Vendor
        {
            Id = Vendor, TenantId = Tenant, VendorCode = "V1", VendorName = "Supply Co"
        });
        db.PurchaseInvoices.Add(new PurchaseInvoice
        {
            Id = Guid.NewGuid(), TenantId = Tenant, VendorId = Vendor,
            InvoiceNumber = "PI-1", InvoiceDate = new DateTime(2026, 4, 5),
            DueDate = new DateTime(2026, 5, 5), Status = "Approved",
            TotalAmount = 200m, TaxAmount = 20m, NetPayable = 200m
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse, TenantId = Tenant, WarehouseCode = "WH1", WarehouseName = "Main"
        });
        db.Items.Add(new Item
        {
            Id = Item, TenantId = Tenant, ItemCode = "SKU1", ItemName = "Bolt"
        });
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(), TenantId = Tenant, ItemId = Item, WarehouseId = Warehouse,
            CurrentQuantity = 15m, ReservedQuantity = 3m
        });
        await db.SaveChangesAsync();

        var svc = new OperationalReportService(db);
        var purchase = await svc.GetPurchaseInvoiceSummaryAsync(
            Tenant, new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));
        Assert.Equal(1, purchase.InvoiceCount);
        Assert.Equal(200m, purchase.TotalAmount);
        Assert.Equal("Supply Co", purchase.Lines[0].VendorName);

        var stock = await svc.GetStockBalanceReportAsync(Tenant, Warehouse);
        Assert.Single(stock.Lines);
        Assert.Equal(15m, stock.Lines[0].CurrentQuantity);
        Assert.Equal(12m, stock.Lines[0].AvailableQuantity);
        Assert.Equal("Bolt", stock.Lines[0].ItemName);
    }
}
