using ERP.Api.GraphQL.Procurement;
using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class GraphQLPurchaseReturnDebitNoteSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesReturnAndDebitNoteQueriesAndCrud()
    {
        var queries = typeof(ProcurementQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(ProcurementMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetPurchaseReturn", queries);
        Assert.Contains("GetPurchaseReturns", queries);
        Assert.Contains("GetPurchaseDebitNote", queries);
        Assert.Contains("GetPurchaseDebitNotes", queries);

        Assert.Contains("UpdatePurchaseReturn", mutations);
        Assert.Contains("DeletePurchaseReturn", mutations);
        Assert.Contains("UpdatePurchaseDebitNote", mutations);
        Assert.Contains("DeletePurchaseDebitNote", mutations);
        Assert.Contains("ApprovePurchaseReturn", mutations);
        Assert.Contains("PostPurchaseDebitNote", mutations);
    }

    [Fact]
    public async Task GraphQL_Queries_ListReturnsAndDebitNotes()
    {
        await using var db = Db();
        var returnId = Guid.NewGuid();
        var dnId = Guid.NewGuid();
        db.PurchaseReturns.Add(new PurchaseReturn
        {
            Id = returnId,
            TenantId = Tenant,
            ReturnNumber = "PR-GQL-1",
            VendorId = Vendor,
            ReturnDate = DateTime.UtcNow.Date,
            Status = "Draft"
        });
        db.PurchaseDebitNotes.Add(new PurchaseDebitNote
        {
            Id = dnId,
            TenantId = Tenant,
            DebitNoteNumber = "PDN-GQL-1",
            VendorId = Vendor,
            DebitNoteDate = DateTime.UtcNow.Date,
            Status = "Draft",
            TotalAmount = 50m
        });
        await db.SaveChangesAsync();

        var bills = new BillService(new BillRepository(db));
        var debitSvc = new PurchaseDebitNoteService(
            new PurchaseDebitNoteRepository(db),
            new PurchaseDebitNoteLineRepository(db),
            new PurchaseInvoiceRepository(db),
            bills);
        var returnSvc = new PurchaseReturnService(
            new PurchaseReturnRepository(db),
            new StockService(new StockBalanceRepository(db)),
            debitSvc);

        var queries = new ProcurementQueries();

        var returns = (await queries.GetPurchaseReturns(returnSvc, Tenant)).ToList();
        Assert.Contains(returns, r => r.Id == returnId);

        var byStatus = (await queries.GetPurchaseReturns(returnSvc, Tenant, status: "Draft")).ToList();
        Assert.Contains(byStatus, r => r.Id == returnId);

        var oneReturn = await queries.GetPurchaseReturn(returnSvc, returnId, Tenant);
        Assert.Equal("PR-GQL-1", oneReturn!.ReturnNumber);

        var notes = (await queries.GetPurchaseDebitNotes(debitSvc, Tenant, vendorId: Vendor)).ToList();
        Assert.Contains(notes, d => d.Id == dnId);

        var oneDn = await queries.GetPurchaseDebitNote(debitSvc, dnId, Tenant);
        Assert.Equal("PDN-GQL-1", oneDn!.DebitNoteNumber);
    }
}
