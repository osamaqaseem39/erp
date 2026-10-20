using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class Wave3BankTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static BankService Svc(ERPDbContext db) =>
        new(new BankRepository(db), new BankAccountRepository(db), db);

    [Fact]
    public async Task Bank_Account_And_Transaction_UpdateBalance()
    {
        await using var db = Db();
        var svc = Svc(db);

        var bankId = await svc.CreateAsync(new CreateBankDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CompanyId = Guid.NewGuid(),
            BankName = "First National",
            BankCode = "FN01",
            IsActive = true
        });
        Assert.NotEqual(Guid.Empty, bankId);

        var loaded = await svc.GetByIdAsync(bankId);
        Assert.Equal("First National", loaded!.BankName);

        var accountId = await svc.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = Tenant,
            BankId = bankId,
            AccountName = "Operating",
            AccountNumber = "ACC-1001",
            AccountType = "Checking",
            Currency = "USD",
            Balance = 100,
            GLAccountId = Guid.NewGuid(),
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, accountId);

        var accounts = (await svc.GetBankAccountsAsync(bankId)).ToList();
        Assert.Single(accounts);
        Assert.Equal(100m, accounts[0].Balance);

        var txId = await svc.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = accountId,
            Amount = 25,
            CurrencyCode = "USD",
            Description = "Deposit",
            TransactionDate = DateTime.UtcNow.Date
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, txId);

        accounts = (await svc.GetBankAccountsAsync(bankId)).ToList();
        Assert.Equal(125m, accounts[0].Balance);

        Assert.False(await svc.DeleteBankAccountAsync(accountId, Tenant));
        Assert.False(await svc.DeleteAsync(bankId));
    }

    [Fact]
    public async Task Match_And_Unmatch_Transaction()
    {
        await using var db = Db();
        var svc = Svc(db);
        var bankId = await svc.CreateAsync(new CreateBankDto
        {
            TenantId = Tenant,
            BankName = "Match Bank",
            BankCode = "MB",
            IsActive = true
        });
        var accountId = await svc.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = Tenant,
            BankId = bankId,
            AccountNumber = "ACC-M1",
            AccountName = "Ops",
            Currency = "USD",
            Balance = 0,
            GLAccountId = Guid.NewGuid()
        }, Tenant, User);

        var txId = await svc.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = accountId,
            Amount = -40,
            CurrencyCode = "USD",
            TransactionDate = DateTime.UtcNow.Date
        }, Tenant, User);

        var paymentId = Guid.NewGuid();
        db.Payments.Add(new Payment
        {
            Id = paymentId,
            TenantId = Tenant,
            PaymentNumber = "PAY-1",
            PaymentDate = DateTime.UtcNow.Date,
            PaymentType = "Bank",
            PaymentMethod = "Bank",
            Amount = 40m,
            Currency = "USD",
            CurrencyCode = "USD",
            Status = "Completed"
        });
        await db.SaveChangesAsync();

        Assert.True(await svc.MatchBankTransactionToPaymentAsync(new BankTransactionMatchDto
        {
            BankTransactionId = txId,
            PaymentId = paymentId,
            MatchAmount = 40
        }, Tenant, User));

        Assert.False(await svc.MatchBankTransactionToPaymentAsync(new BankTransactionMatchDto
        {
            BankTransactionId = txId,
            PaymentId = Guid.NewGuid(),
            MatchAmount = 40
        }, Tenant, User));

        Assert.True(await svc.UnmatchBankTransactionAsync(txId, Tenant, User));
        Assert.True(await svc.MatchBankTransactionToPaymentAsync(new BankTransactionMatchDto
        {
            BankTransactionId = txId,
            PaymentId = paymentId,
            MatchAmount = 40
        }, Tenant, User));
    }

    [Fact]
    public async Task Reconciliation_Open_Tick_Close_When_Balanced()
    {
        await using var db = Db();
        var svc = Svc(db);
        var bankId = await svc.CreateAsync(new CreateBankDto
        {
            TenantId = Tenant,
            BankName = "Recon Bank",
            BankCode = "RB",
            IsActive = true
        });
        var accountId = await svc.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = Tenant,
            BankId = bankId,
            AccountNumber = "ACC-R1",
            AccountName = "Ops",
            Currency = "USD",
            Balance = 100,
            GLAccountId = Guid.NewGuid()
        }, Tenant, User);

        var day = new DateTime(2026, 9, 15);
        var deposit = await svc.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = accountId,
            Amount = 50,
            CurrencyCode = "USD",
            TransactionDate = day
        }, Tenant, User);
        var withdrawal = await svc.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = accountId,
            Amount = -20,
            CurrencyCode = "USD",
            TransactionDate = day
        }, Tenant, User);

        // Opening 100, statement closing 130 (= 100 + 50 - 20)
        var reconId = await svc.CreateReconciliationAsync(new CreateBankReconciliationDto
        {
            BankAccountId = accountId,
            ReconciliationDate = day,
            OpeningBalance = 100,
            ClosingBalance = 130
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, reconId);

        Assert.Equal(Guid.Empty, await svc.CreateReconciliationAsync(new CreateBankReconciliationDto
        {
            BankAccountId = accountId,
            ReconciliationDate = day,
            OpeningBalance = 100,
            ClosingBalance = 130
        }, Tenant, User));

        Assert.False(await svc.CloseReconciliationAsync(reconId, Tenant, User));

        Assert.True(await svc.SetTransactionReconciledAsync(reconId, deposit, true, Tenant, User));
        Assert.True(await svc.SetTransactionReconciledAsync(reconId, withdrawal, true, Tenant, User));

        var open = await svc.GetReconciliationAsync(reconId, Tenant);
        Assert.Equal(0m, open!.Difference);
        Assert.Equal(2, open.ReconciledTransactionCount);

        Assert.True(await svc.CloseReconciliationAsync(reconId, Tenant, User));
        var closed = await svc.GetReconciliationAsync(reconId, Tenant);
        Assert.Equal("Closed", closed!.Status);
        Assert.False(await svc.SetTransactionReconciledAsync(reconId, deposit, false, Tenant, User));
    }

    [Fact]
    public async Task GetAllBankAccounts_And_GetById_Are_Tenant_Scoped()
    {
        await using var db = Db();
        var svc = Svc(db);
        var otherTenant = Guid.NewGuid();

        var bankA = await svc.CreateAsync(new CreateBankDto
        {
            TenantId = Tenant,
            BankName = "A Bank",
            BankCode = "A1",
            IsActive = true
        });
        var accountA = await svc.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = Tenant,
            BankId = bankA,
            AccountNumber = "ACC-A",
            AccountName = "A Ops",
            Currency = "USD",
            Balance = 10,
            GLAccountId = Guid.NewGuid()
        }, Tenant, User);

        var bankB = await svc.CreateAsync(new CreateBankDto
        {
            TenantId = otherTenant,
            BankName = "B Bank",
            BankCode = "B1",
            IsActive = true
        });
        await svc.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = otherTenant,
            BankId = bankB,
            AccountNumber = "ACC-B",
            AccountName = "B Ops",
            Currency = "USD",
            Balance = 99,
            GLAccountId = Guid.NewGuid()
        }, otherTenant, User);

        var allA = (await svc.GetAllBankAccountsAsync(Tenant)).ToList();
        Assert.Single(allA);
        Assert.Equal("ACC-A", allA[0].AccountNumber);

        var byId = await svc.GetBankAccountByIdAsync(accountA, Tenant);
        Assert.NotNull(byId);
        Assert.Equal("A Ops", byId!.AccountName);

        Assert.Null(await svc.GetBankAccountByIdAsync(accountA, otherTenant));
        Assert.Empty(await svc.GetAllBankAccountsAsync(Guid.Empty));
    }
}
