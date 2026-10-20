using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class ChequeRegisterTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Acc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db), db);

    [Fact]
    public async Task Register_Lists_Cheques_And_Clear_Bounce()
    {
        await using var db = Db();
        var svc = Acc(db);
        var bank = Guid.NewGuid();
        var expense = Guid.NewGuid();
        var income = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = bank, AccountId = bank, TenantId = Tenant,
                AccountCode = "1120", AccountName = "Operating Bank", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expense, AccountId = expense, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Supplies", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = income, AccountId = income, TenantId = Tenant,
                AccountCode = "4000", AccountName = "Sales", AccountType = "Revenue",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var day = new DateTime(2026, 10, 12);
        var payId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "BankPayment",
            JournalDate = day,
            CashOrBankAccountId = bank,
            ChequeNumber = "CHQ-1001",
            PaidBy = "Vendor A",
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 250m }]
        }, Tenant, User);
        await svc.ApproveJournalAsync(payId, Tenant, Approver);
        await svc.PostJournalAsync(payId, Tenant, Approver, allowPostWithoutApprove: false);

        var recvId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "BankReceipt",
            JournalDate = day,
            CashOrBankAccountId = bank,
            ChequeNumber = "CHQ-2002",
            ReceivedBy = "Customer B",
            CreditLines = [new CreateVoucherLineDto { AccountId = income, Amount = 400m }]
        }, Tenant, User);

        // No cheque — must not appear
        await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "BankPayment",
            JournalDate = day,
            CashOrBankAccountId = bank,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 10m }]
        }, Tenant, User);

        var register = await svc.GetChequeRegisterAsync(Tenant, day, day);
        Assert.Equal(2, register.TotalCount);
        Assert.Contains(register.Lines, l => l.ChequeNumber == "CHQ-1001" && l.ChequeStatus == "Outstanding" && l.Amount == 250m);
        Assert.Contains(register.Lines, l => l.ChequeNumber == "CHQ-2002" && l.Direction == "Inflow");
        Assert.Equal(650m, register.OutstandingAmount);

        Assert.True(await svc.SetChequeStatusAsync(payId, new SetChequeStatusDto
        {
            Status = "Cleared",
            Notes = "Bank cleared"
        }, Tenant, User));

        Assert.True(await svc.SetChequeStatusAsync(recvId, new SetChequeStatusDto
        {
            Status = "Bounced",
            Notes = "NSF"
        }, Tenant, User));

        var after = await svc.GetChequeRegisterAsync(Tenant, day, day);
        Assert.Equal(250m, after.ClearedAmount);
        Assert.Equal(400m, after.BouncedAmount);
        Assert.Equal(0m, after.OutstandingAmount);

        var outstandingOnly = await svc.GetChequeRegisterAsync(Tenant, day, day, chequeStatus: "Outstanding");
        Assert.Empty(outstandingOnly.Lines);

        var cleared = await svc.GetChequeRegisterAsync(Tenant, day, day, chequeStatus: "Cleared");
        Assert.Single(cleared.Lines);
        Assert.Equal("Bank cleared", cleared.Lines[0].ChequeStatusNotes);

        Assert.False(await svc.SetChequeStatusAsync(payId, new SetChequeStatusDto { Status = "Bogus" }, Tenant, User));
    }
}
