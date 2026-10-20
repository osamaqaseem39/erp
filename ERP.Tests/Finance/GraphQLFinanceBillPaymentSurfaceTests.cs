using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLFinanceBillPaymentSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Mutations_ExposeBillApproveVoid_AndPaymentVoidRefund()
    {
        var names = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ApproveBill", names);
        Assert.Contains("MarkBillAsPaid", names);
        Assert.Contains("VoidBill", names);
        Assert.Contains("VoidInvoice", names);
        Assert.Contains("VoidPayment", names);
        Assert.Contains("RefundPayment", names);

        var queryNames = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("GetBill", queryNames);
        Assert.Contains("SearchBills", queryNames);
        Assert.Contains("GetBillLines", queryNames);
    }

    [Fact]
    public async Task ApproveBill_Then_VoidBill_ViaGraphQLMutations()
    {
        await using var db = Db();
        var billId = Guid.NewGuid();
        db.Bills.Add(new Bill
        {
            Id = billId,
            BillId = billId,
            TenantId = Tenant,
            VendorId = Vendor,
            BillNumber = "AP-GQL-1",
            BillDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 100,
            TaxAmount = 0,
            TotalAmount = 100,
            Balance = 100,
            Status = "Draft",
            IsLocked = false
        });
        await db.SaveChangesAsync();

        var bills = new BillService(new BillRepository(db));
        var gql = new FinanceMutations(null!, null!, bills, null!, null!, null!);

        var approved = await gql.ApproveBill(billId, Tenant, User);
        Assert.Equal("Issued", approved.Status);

        var voided = await gql.VoidBill(billId, new VoidBillDto { Reason = "test" }, Tenant, User);
        Assert.Equal("Cancelled", voided.Status);
        Assert.False(voided.IsActive);
    }
}
