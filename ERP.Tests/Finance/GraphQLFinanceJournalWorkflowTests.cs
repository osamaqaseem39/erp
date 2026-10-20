using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

/// <summary>GraphQL FinanceMutations journal workflow parity with REST approve/reject/unpost/cheque.</summary>
public class GraphQLFinanceJournalWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Cash = Guid.NewGuid();
    private static readonly Guid Expense = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static FinanceMutations Mutations(ERPDbContext db)
    {
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        return new FinanceMutations(accounting, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Approve_Post_Unpost_Reject_And_ChequeStatus_ViaGraphQLMutations()
    {
        await using var db = Db();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = Cash, AccountId = Cash, TenantId = Tenant,
                AccountCode = "1000", AccountName = "Cash", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = Expense, AccountId = Expense, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Expense", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var gql = Mutations(db);
        var journalId = await gql.AddJournal(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            Description = "GQL workflow",
            CashOrBankAccountId = Cash,
            ChequeNumber = "CHQ-1",
            DebitLines =
            [
                new CreateCashPaymentLineDto { AccountId = Expense, Amount = 100m }
            ]
        }, Tenant, Creator);
        Assert.NotEqual(Guid.Empty, journalId);

        Assert.Equal(Guid.Empty, await gql.ApproveJournal(journalId, Tenant, Creator));
        Assert.Equal(journalId, await gql.ApproveJournal(journalId, Tenant, Approver));

        Assert.Equal(journalId, await gql.PostJournal(journalId, Tenant, Approver));
        var afterPost = await new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db).GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Posted", afterPost!.Status);

        Assert.Equal(journalId, await gql.UnpostJournal(journalId, Tenant, Approver));
        afterPost = await new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db).GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Draft", afterPost!.Status);

        Assert.Equal(journalId, await gql.ApproveJournal(journalId, Tenant, Approver));
        Assert.Equal(journalId, await gql.RejectJournal(journalId, Tenant, Approver, "fix"));
        afterPost = await new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db).GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Draft", afterPost!.Status);

        // Re-approve + post so cheque status can update on a voucher with ChequeNumber
        Assert.Equal(journalId, await gql.ApproveJournal(journalId, Tenant, Approver));
        Assert.Equal(journalId, await gql.PostJournal(journalId, Tenant, Approver));

        var withCheque = await gql.SetJournalChequeStatus(
            journalId,
            new SetChequeStatusDto { Status = "Cleared", Notes = "bank" },
            Tenant,
            Approver);
        Assert.NotNull(withCheque);
        Assert.Equal("Cleared", withCheque!.ChequeStatus);
    }
}
