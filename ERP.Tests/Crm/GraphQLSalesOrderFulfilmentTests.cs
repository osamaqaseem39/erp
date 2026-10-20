using ERP.Api.GraphQL.CRM;
using ERP.Application.CRM;
using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class GraphQLSalesOrderFulfilmentTests
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
            Task.FromResult($"SO-GQL-{Interlocked.Increment(ref _n)}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class StubInvoices : ERP.Application.Interfaces.Finance.IInvoiceService
    {
        public Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId) =>
            Task.FromResult(Guid.NewGuid());
        public Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
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
        public Task<Guid> AddBillLineAsync(BillLineCreateDto dto, Guid billId, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<Guid> UpdateBillLineAsync(BillLineUpdateDto dto, Guid tenantId, Guid userId) => Task.FromResult(Guid.Empty);
        public Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId) => Task.FromResult(false);
        public Task<IEnumerable<BillLineDto>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId) =>
            Task.FromResult(Enumerable.Empty<BillLineDto>());
        public Task<bool> VoidInvoiceAsync(Guid invoiceId, VoidInvoiceDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<bool> ApplyCreditToInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId) => Task.FromResult(false);
        public Task<bool> UnapplyCreditFromInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId) => Task.FromResult(false);
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

    private static SalesOrderService Svc(ERPDbContext db) => new(
        new SalesOrderRepository(db, new FixedTenant()),
        new SalesOrderLineRepository(db, new FixedTenant()),
        new StubInvoices(),
        new StubNumbers(),
        new StockService(new StockBalanceRepository(db), tenant: new FixedTenant()));

    [Fact]
    public void Surface_ExposesShipAndInvoiceMutations()
    {
        var mutations = typeof(CRMMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(CRMMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ShipSalesOrder", mutations);
        Assert.Contains("InvoiceSalesOrder", mutations);
        Assert.Contains("ApproveSalesOrder", mutations);
        Assert.Contains("RejectSalesOrder", mutations);
    }

    [Fact]
    public async Task GraphQL_Approve_Ship_Invoice()
    {
        await using var db = Db();
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 10,
            ReservedQuantity = 0
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var mutations = new CRMMutations();
        var queries = new CRMQueries();

        var id = await svc.CreateAsync(new CreateSalesOrderDto
        {
            TenantId = Tenant,
            CreatedBy = Creator.ToString(),
            CustomerId = Customer,
            OrderDate = DateTime.UtcNow.Date,
            SalesOrderLines =
            [
                new CreateSalesOrderLineDto { ItemId = Item, Quantity = 3, UnitPrice = 10, TaxRate = 0 }
            ]
        });

        Assert.False(await mutations.ShipSalesOrder(svc, id, Tenant, Approver));
        Assert.True(await mutations.ApproveSalesOrder(svc, id, Tenant, Approver));
        Assert.True(await mutations.ShipSalesOrder(svc, id, Tenant, Approver));
        Assert.Equal("Shipped", (await queries.GetSalesOrder(svc, id))!.Status);
        Assert.True(await mutations.InvoiceSalesOrder(svc, id, Tenant, Approver));
        Assert.Equal("Invoiced", (await queries.GetSalesOrder(svc, id))!.Status);
    }
}
