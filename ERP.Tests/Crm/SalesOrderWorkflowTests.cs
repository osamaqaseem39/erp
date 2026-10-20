using ERP.Application.CRM;
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

public class SalesOrderWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
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
            Task.FromResult($"SO-WF-{Interlocked.Increment(ref _n)}");
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

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task SeedStockAsync(ERPDbContext db, decimal qty = 100)
    {
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = qty,
            ReservedQuantity = 0,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static SalesOrderService Svc(ERPDbContext db)
    {
        var stock = new StockService(new StockBalanceRepository(db), tenant: new FixedTenant());
        return new(
            new SalesOrderRepository(db, new FixedTenant()),
            new SalesOrderLineRepository(db, new FixedTenant()),
            new StubInvoices(),
            new StubNumbers(),
            stock,
            new StockReservationService(new StockReservationRepository(db), stock),
            new ItemKitService(new ItemKitRepository(db)),
            new StubItems());
    }

    private static async Task<Guid> CreateDraftAsync(SalesOrderService svc, decimal qty = 2)
    {
        return await svc.CreateAsync(new CreateSalesOrderDto
        {
            TenantId = Tenant,
            CreatedBy = Creator.ToString(),
            CustomerId = Customer,
            OrderDate = DateTime.UtcNow.Date,
            Status = "Approved", // ignored — always Draft
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto { ItemId = Item, Quantity = qty, UnitPrice = 10, TaxRate = 0 }
            ]
        }, Tenant, Creator);
    }

    [Fact]
    public async Task Create_Always_Draft_And_Creator_Cannot_Approve()
    {
        await using var db = Db();
        await SeedStockAsync(db);
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.NotNull((await svc.GetByIdAsync(id, Tenant))!.ApprovedDate);
    }

    [Fact]
    public async Task Reject_From_Draft_Or_Approved_And_Blocks_Ship()
    {
        await using var db = Db();
        await SeedStockAsync(db);
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.True(await svc.RejectAsync(id, Tenant, Approver));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.ShipAsync(id, Tenant, Approver));
        Assert.False(await svc.ApproveAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Draft_Only_Editable_After_Approve()
    {
        await using var db = Db();
        await SeedStockAsync(db);
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateSalesOrderDto
        {
            Id = id,
            Notes = "ok",
            Status = "Shipped",
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto { ItemId = Item, Quantity = 1, UnitPrice = 5 }
            ]
        }, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.False(await svc.UpdateAsync(new UpdateSalesOrderDto { Id = id, Notes = "nope", SalesOrderLines = [] }, Tenant, Creator));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Approve_Ship_Invoice_HappyPath()
    {
        await using var db = Db();
        await SeedStockAsync(db, 10);

        var svc = Svc(db);
        var id = await CreateDraftAsync(svc, qty: 3);

        Assert.False(await svc.ShipAsync(id, Tenant, Approver)); // must approve first
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.True(await svc.ShipAsync(id, Tenant, Approver));
        Assert.Equal(7m, await new StockService(new StockBalanceRepository(db), tenant: new FixedTenant()).GetStockLevelAsync(Item, Warehouse));
        Assert.False(await svc.RejectAsync(id, Tenant, Approver)); // shipped cannot cancel
        Assert.True(await svc.InvoiceAsync(id, Tenant, Approver));
        Assert.Equal("Invoiced", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.True(await svc.InvoiceAsync(id, Tenant, Approver)); // idempotent
    }

    [Fact]
    public async Task Tenant_Mismatch_Blocks_Workflow()
    {
        await using var db = Db();
        await SeedStockAsync(db);
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);
        Assert.False(await svc.ApproveAsync(id, Guid.NewGuid(), Approver));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }
}
