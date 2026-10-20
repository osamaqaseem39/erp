using ERP.Application.CRM;
using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Sales;
using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class DualTaxTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private sealed class StubItems : IItemService
    {
        public Task<ItemDto?> GetByIdAsync(Guid id, Guid tenantId) =>
            Task.FromResult<ItemDto?>(new ItemDto { Id = id, TenantId = tenantId });
        public Task<IEnumerable<ItemDto>> GetAllAsync(Guid tenantId, Guid companyId) =>
            Task.FromResult(Enumerable.Empty<ItemDto>());
        public Task<IEnumerable<ItemDto>> GetByCategoryAsync(Guid categoryId, Guid tenantId) =>
            Task.FromResult(Enumerable.Empty<ItemDto>());
        public Task<ItemDto> CreateAsync(CreateItemDto dto, Guid tenantId, string createdBy) =>
            throw new NotImplementedException();
        public Task<ItemDto> UpdateAsync(UpdateItemDto dto, Guid tenantId, string updatedBy) =>
            throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid id, Guid tenantId) => Task.FromResult(false);
        public Task<bool> ExistsAsync(Guid id, Guid tenantId) => Task.FromResult(true);
        public Task<IEnumerable<ItemDto>> GetLowStockItemsAsync(Guid tenantId, Guid companyId) =>
            Task.FromResult(Enumerable.Empty<ItemDto>());
    }

    private sealed class StubNumbers : IDocumentNumberService
    {
        private int _n;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult($"SO-DT-{Interlocked.Increment(ref _n)}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class StubInvoices : ERP.Application.Interfaces.Finance.IInvoiceService
    {
        public Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.NewGuid());
        public Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<bool> DeleteInvoiceAsync(Guid invoiceId, Guid tenantId) => Task.FromResult(false);
        public Task<Guid> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy) => Task.FromResult(Guid.Empty);
        public Task<bool> SendInvoiceAsync(Guid invoiceId, Guid tenantId, Guid userId, CancellationToken ct = default) =>
            Task.FromResult(false);
        public Task<bool> VoidInvoiceAsync(Guid invoiceId, VoidInvoiceDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId) => Task.FromResult<InvoiceDto?>(null);
        public Task<(IEnumerable<InvoiceDto> Invoices, int TotalCount)> SearchInvoicesAsync(InvoiceQuery query) =>
            Task.FromResult((Enumerable.Empty<InvoiceDto>(), 0));
        public Task<Guid> AddInvoiceLineAsync(InvoiceLineCreateDto dto, Guid invoiceId, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<Guid> UpdateInvoiceLineAsync(InvoiceLineUpdateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<bool> DeleteInvoiceLineAsync(Guid invoiceLineId, Guid tenantId) => Task.FromResult(false);
        public Task<IEnumerable<InvoiceLineDto>> GetInvoiceLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId) =>
            Task.FromResult(Enumerable.Empty<InvoiceLineDto>());
        public Task<Guid> AddBillAsync(BillCreateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<Guid> UpdateBillAsync(BillUpdateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<bool> DeleteBillAsync(Guid billId, Guid tenantId) => Task.FromResult(false);
        public Task<Guid> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy) => Task.FromResult(Guid.Empty);
        public Task<BillDto?> GetBillByIdAsync(Guid billId, Guid tenantId) => Task.FromResult<BillDto?>(null);
        public Task<(IEnumerable<BillDto> Bills, int TotalCount)> SearchBillsAsync(BillQuery query) =>
            Task.FromResult((Enumerable.Empty<BillDto>(), 0));
        public Task<Guid> AddBillLineAsync(BillLineCreateDto dto, Guid billId, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<Guid> UpdateBillLineAsync(BillLineUpdateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId) => Task.FromResult(false);
        public Task<IEnumerable<BillLineDto>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId) =>
            Task.FromResult(Enumerable.Empty<BillLineDto>());
        public Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<bool> ApplyCreditToInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId) => Task.FromResult(false);
        public Task<bool> UnapplyCreditFromInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId) => Task.FromResult(false);
        public Task<AgingReportDto> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto());
        public Task<AgingReportDto> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto());
        public Task<AgingReportDto> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto());
        public Task<AgingReportDto> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto());
        public Task<PartnerBalanceSummaryReportDto> GetPartnerBalanceSummaryAsync(Guid tenantId, DateTime asOfDate) =>
            Task.FromResult(new PartnerBalanceSummaryReportDto());
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Calculator_Applies_Both_Taxes_On_Net()
    {
        var taxes = CrmLineCalculator.CalculateLineTaxes(1000m, 10m, 2m);
        Assert.Equal(100m, taxes.Tax1Amount);
        Assert.Equal(20m, taxes.Tax2Amount);
        Assert.Equal(120m, taxes.TaxTotal);

        var totals = CrmLineCalculator.CalculateTotals(new[] { (1000m, 10m, 2m) });
        Assert.Equal(1000m, totals.SubTotal);
        Assert.Equal(100m, totals.Tax1Amount);
        Assert.Equal(20m, totals.Tax2Amount);
        Assert.Equal(120m, totals.TaxAmount);
        Assert.Equal(1120m, totals.TotalAmount);
    }

    [Fact]
    public void Calculator_Legacy_Single_Tax_Still_Works()
    {
        var totals = CrmLineCalculator.CalculateTotals(new[] { (200m, 5m) });
        Assert.Equal(200m, totals.SubTotal);
        Assert.Equal(10m, totals.TaxAmount);
        Assert.Equal(210m, totals.TotalAmount);
    }

    [Fact]
    public async Task SalesInvoice_Persists_Dual_Tax()
    {
        await using var db = Db();
        var svc = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db)),
            new TestCoreSettings());

        var id = await svc.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-DT-1",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Lines =
            [
                new CreateSalesInvoiceLineDto
                {
                    ItemId = Item,
                    Description = "Widget",
                    Quantity = 2,
                    UnitPrice = 50,
                    Tax1Rate = 10,
                    Tax2Rate = 2,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        var loaded = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Equal(100m, loaded!.SubTotal);
        Assert.Equal(10m, loaded.Tax1Amount);
        Assert.Equal(2m, loaded.Tax2Amount);
        Assert.Equal(12m, loaded.TaxAmount);
        Assert.Equal(112m, loaded.TotalAmount);

        var line = Assert.Single(loaded.Lines);
        Assert.Equal(10m, line.Tax1Rate);
        Assert.Equal(10m, line.Tax1Amount);
        Assert.Equal(2m, line.Tax2Rate);
        Assert.Equal(2m, line.Tax2Amount);
        Assert.Equal(12m, line.TaxAmount);
        Assert.Equal(112m, line.LineTotal);
    }

    [Fact]
    public async Task SalesOrder_Persists_Dual_Tax()
    {
        await using var db = Db();
        var tenant = new FixedTenant();
        var stock = new StockService(new StockBalanceRepository(db));
        var svc = new SalesOrderService(
            new SalesOrderRepository(db, tenant),
            new SalesOrderLineRepository(db, tenant),
            new StubInvoices(),
            new StubNumbers(),
            stock,
            new StockReservationService(new StockReservationRepository(db), stock),
            new ItemKitService(new ItemKitRepository(db)),
            new StubItems());

        var id = await svc.CreateAsync(new CreateSalesOrderDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CustomerId = Customer,
            OrderDate = DateTime.UtcNow.Date,
            Status = "Draft",
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto
                {
                    ItemId = Item,
                    Description = "Part",
                    Quantity = 10,
                    UnitPrice = 100,
                    DiscountPercent = 10, // net 900
                    Tax1Rate = 10,
                    Tax2Rate = 5
                }
            ]
        }, Tenant, User);

        var loaded = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Equal(900m, loaded!.SubTotal);
        Assert.Equal(90m, loaded.Tax1Amount);
        Assert.Equal(45m, loaded.Tax2Amount);
        Assert.Equal(135m, loaded.TaxAmount);
        Assert.Equal(1035m, loaded.TotalAmount);

        var line = Assert.Single(loaded.SalesOrderLines);
        Assert.Equal(10m, line.Tax1Rate);
        Assert.Equal(90m, line.Tax1Amount);
        Assert.Equal(5m, line.Tax2Rate);
        Assert.Equal(45m, line.Tax2Amount);
        Assert.Equal(900m, line.TotalPrice);
    }
}
