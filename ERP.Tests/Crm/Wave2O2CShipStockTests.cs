using ERP.Application.CRM;
using ERP.Application.Interfaces.Inventory;
using ERP.Application.Inventory;
using ERP.Application.SCM.Interfaces;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class Wave2O2CShipStockTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private sealed class StubNumbers : IDocumentNumberService
    {
        private int _n;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult($"SO-W2-{Interlocked.Increment(ref _n)}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class StubInvoices : ERP.Application.Interfaces.Finance.IInvoiceService
    {
        public Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.NewGuid());
        public Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.Empty);
        public Task<bool> DeleteInvoiceAsync(Guid invoiceId, Guid tenantId) => Task.FromResult(false);
        public Task<Guid> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy) => Task.FromResult(Guid.Empty);
        public Task<bool> SendInvoiceAsync(Guid invoiceId, Guid tenantId, Guid userId, CancellationToken ct = default) =>
            Task.FromResult(false);
        public Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId) => Task.FromResult<InvoiceDto?>(null);
        public Task<(IEnumerable<InvoiceDto> Invoices, int TotalCount)> SearchInvoicesAsync(InvoiceQuery query) =>
            Task.FromResult((Enumerable.Empty<InvoiceDto>(), 0));
        public Task<Guid> AddInvoiceLineAsync(InvoiceLineCreateDto dto, Guid invoiceId, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.Empty);
        public Task<Guid> UpdateInvoiceLineAsync(InvoiceLineUpdateDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.Empty);
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
        public Task<Guid> AddBillLineAsync(BillLineCreateDto dto, Guid billId, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.Empty);
        public Task<Guid> UpdateBillLineAsync(BillLineUpdateDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.Empty);
        public Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId) => Task.FromResult(false);
        public Task<IEnumerable<BillLineDto>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId) =>
            Task.FromResult(Enumerable.Empty<BillLineDto>());
        public Task<bool> VoidInvoiceAsync(Guid invoiceId, VoidInvoiceDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(false);
        public Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(false);
        public Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(false);
        public Task<bool> ApplyCreditToInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId) =>
            Task.FromResult(false);
        public Task<bool> UnapplyCreditFromInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId) =>
            Task.FromResult(false);
        public Task<AgingReportDto> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto { Kind = "Receivables", AsOfDate = asOfDate });
        public Task<AgingReportDto> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto { Kind = "Payables", AsOfDate = asOfDate });
        public Task<AgingReportDto> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto { Kind = "Receivables", AsOfDate = asOfDate });
        public Task<AgingReportDto> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null) =>
            Task.FromResult(new AgingReportDto { Kind = "Payables", AsOfDate = asOfDate });
        public Task<PartnerBalanceSummaryReportDto> GetPartnerBalanceSummaryAsync(Guid tenantId, DateTime asOfDate) =>
            Task.FromResult(new PartnerBalanceSummaryReportDto { AsOfDate = asOfDate });
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static SalesOrderService CreateService(ERPDbContext db, StockService stock) =>
        new(
            new SalesOrderRepository(db, new FixedTenant()),
            new SalesOrderLineRepository(db, new FixedTenant()),
            new StubInvoices(),
            new StubNumbers(),
            stock,
            new StockReservationService(new StockReservationRepository(db), stock),
            new ItemKitService(new ItemKitRepository(db)),
            new StubItems());

    private sealed class StubItems : IItemService
    {
        public Task<ItemDto?> GetByIdAsync(Guid id, Guid tenantId) =>
            Task.FromResult<ItemDto?>(new ItemDto { Id = id, TenantId = tenantId, DefaultWarehouseId = Warehouse });
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

    [Fact]
    public async Task Ship_IssuesStock_FromAnyWarehouse()
    {
        await using var db = Db();
        var stockRepo = new StockBalanceRepository(db);
        var stock = new StockService(stockRepo, tenant: new FixedTenant());

        await stockRepo.CreateAsync(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 8,
            ReservedQuantity = 0,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        var svc = CreateService(db, stock);
        var creator = Guid.NewGuid();
        var approver = Guid.NewGuid();
        var orderId = await svc.CreateAsync(new CreateSalesOrderDto
        {
            TenantId = Tenant,
            CreatedBy = creator.ToString(),
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Status = "Draft",
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto { ItemId = Item, Quantity = 3, UnitPrice = 10, TaxRate = 0 }
            ]
        }, Tenant, creator);
        Assert.True(await svc.ApproveAsync(orderId, Tenant, approver));

        Assert.True(await svc.ShipAsync(orderId, Tenant, approver));
        Assert.Equal("Shipped", (await svc.GetByIdAsync(orderId, Tenant))!.Status);
        Assert.Equal(5m, await stock.GetStockLevelAsync(Item, Warehouse));
        Assert.False(await svc.ShipAsync(orderId, Tenant, approver));
    }

    [Fact]
    public async Task Approve_Fails_WhenInsufficientStock()
    {
        await using var db = Db();
        var stock = new StockService(new StockBalanceRepository(db), tenant: new FixedTenant());
        var svc = CreateService(db, stock);

        var creator = Guid.NewGuid();
        var approver = Guid.NewGuid();
        var orderId = await svc.CreateAsync(new CreateSalesOrderDto
        {
            TenantId = Tenant,
            CreatedBy = creator.ToString(),
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Status = "Draft",
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto { ItemId = Item, Quantity = 2, UnitPrice = 10 }
            ]
        }, Tenant, creator);
        Assert.False(await svc.ApproveAsync(orderId, Tenant, approver));
        Assert.Equal("Draft", (await svc.GetByIdAsync(orderId, Tenant))!.Status);
    }

    [Fact]
    public async Task Ship_Fails_WhenInsufficientStock_AfterApproveReleased()
    {
        await using var db = Db();
        var stockRepo = new StockBalanceRepository(db);
        var stock = new StockService(stockRepo, tenant: new FixedTenant());
        await stockRepo.CreateAsync(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 2,
            ReservedQuantity = 0,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        var svc = CreateService(db, stock);
        var creator = Guid.NewGuid();
        var approver = Guid.NewGuid();
        var orderId = await svc.CreateAsync(new CreateSalesOrderDto
        {
            TenantId = Tenant,
            CreatedBy = creator.ToString(),
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Status = "Draft",
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto { ItemId = Item, Quantity = 2, UnitPrice = 10 }
            ]
        }, Tenant, creator);
        Assert.True(await svc.ApproveAsync(orderId, Tenant, approver));

        // Drain stock outside the reservation path so ship fails after release.
        var bal = (await stockRepo.GetByItemAsync(Item, Tenant)).First();
        bal.CurrentQuantity = 0;
        bal.ReservedQuantity = 0;
        await stockRepo.UpdateAsync(bal);

        Assert.False(await svc.ShipAsync(orderId, Tenant, approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(orderId, Tenant))!.Status);
    }
}
