using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class FinanceGapSetTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task AddInvoice_PersistsSourceSalesInvoiceId_AndIssuedStatus()
    {
        await using var db = Db();
        var siId = Guid.NewGuid();
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var id = await invoices.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-SI-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 100,
            TaxAmount = 0,
            TotalAmount = 100,
            Status = "Issued",
            SourceSalesInvoiceId = siId,
            Notes = $"SalesInvoice:{siId}"
        }, Tenant, User);

        var ar = await invoices.GetInvoiceByIdAsync(id, Tenant);
        Assert.NotNull(ar);
        Assert.Equal(siId, ar!.SourceSalesInvoiceId);
        Assert.Equal("Issued", ar.Status);
        Assert.Contains(siId.ToString(), ar.Notes ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApplyPayment_CreatesXorPaymentApplication_ForInvoiceAndBill()
    {
        await using var db = Db();
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var invoiceId = await invoices.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-XOR-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 50,
            TotalAmount = 50,
            Status = "Issued"
        }, Tenant, User);

        var bills = new BillService(new BillRepository(db));
        var bill = await bills.CreateAsync(new CreateBillDto
        {
            VendorId = Vendor,
            BillNumber = "AP-XOR-1",
            BillDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(15),
            SubTotal = 80,
            TotalAmount = 80,
            Status = "Issued",
            PurchaseInvoiceId = Guid.NewGuid()
        }, Tenant, User);

        var payments = new PaymentService(
            new PaymentRepositoryAdapter(new PaymentRepository(db)),
            new ApplicationInvoiceRepositoryAdapter(db),
            db);

        Assert.True(await payments.ApplyPaymentToInvoiceAsync(new PaymentApplicationDto
        {
            InvoiceId = invoiceId,
            Amount = 50
        }, Tenant, User));

        Assert.True(await payments.ApplyPaymentToBillAsync(new PaymentApplicationDto
        {
            BillId = bill.Id,
            Amount = 40
        }, Tenant, User));

        var apps = await db.PaymentApplications.AsNoTracking()
            .Where(a => a.TenantId == Tenant)
            .ToListAsync();
        Assert.Equal(2, apps.Count);
        Assert.Contains(apps, a => a.InvoiceId == invoiceId && a.BillId == null);
        Assert.Contains(apps, a => a.BillId == bill.Id && a.InvoiceId == null);

        var persistedBill = await bills.GetByIdAsync(bill.Id, Tenant);
        Assert.NotNull(persistedBill);
        Assert.Equal(bill.SourcePurchaseInvoiceId, persistedBill!.SourcePurchaseInvoiceId);
        Assert.Equal(40m, persistedBill.Balance);
    }

    [Fact]
    public async Task BankReconTick_CreatesReconciliationLine()
    {
        await using var db = Db();
        var bankSvc = new BankService(new BankRepository(db), new BankAccountRepository(db), db);
        var bankId = await bankSvc.CreateAsync(new CreateBankDto
        {
            TenantId = Tenant,
            BankName = "Gap Bank",
            BankCode = "GB",
            IsActive = true
        });
        var acctId = await bankSvc.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = Tenant,
            BankId = bankId,
            AccountNumber = "ACC-G1",
            AccountName = "Ops",
            Currency = "USD",
            Balance = 0,
            GLAccountId = Guid.NewGuid()
        }, Tenant, User);
        var day = DateTime.UtcNow.Date;
        var txId = await bankSvc.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = acctId,
            Amount = 25m,
            CurrencyCode = "USD",
            TransactionDate = day
        }, Tenant, User);

        var reconId = await bankSvc.CreateReconciliationAsync(new CreateBankReconciliationDto
        {
            BankAccountId = acctId,
            ReconciliationDate = day,
            OpeningBalance = 0,
            ClosingBalance = 25m
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, reconId);

        Assert.True(await bankSvc.SetTransactionReconciledAsync(reconId, txId, true, Tenant, User));

        var line = await db.BankReconciliationLines.AsNoTracking()
            .FirstOrDefaultAsync(l => l.ReconciliationId == reconId && l.BankTransactionId == txId);
        Assert.NotNull(line);
        Assert.True(line!.IsMatched);
        Assert.Equal(25m, line.MatchedAmount);
    }

    [Fact]
    public async Task GapService_TaxGroup_BudgetLine_CreditMemo_FundTransfer()
    {
        await using var db = Db();
        var revenueId = Guid.NewGuid();
        var arId = Guid.NewGuid();
        var cashFrom = Guid.NewGuid();
        var cashTo = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                AccountId = revenueId, Id = revenueId, TenantId = Tenant, AccountCode = "4000",
                AccountName = "Revenue", AccountType = "Income", IsPostingAllowed = true, CreatedBy = User.ToString()
            },
            new ChartOfAccount
            {
                AccountId = arId, Id = arId, TenantId = Tenant, AccountCode = "1100",
                AccountName = "AR", AccountType = "Asset", IsPostingAllowed = true, CreatedBy = User.ToString()
            },
            new ChartOfAccount
            {
                AccountId = cashFrom, Id = cashFrom, TenantId = Tenant, AccountCode = "1000",
                AccountName = "Cash A", AccountType = "Asset", IsPostingAllowed = true, CreatedBy = User.ToString()
            },
            new ChartOfAccount
            {
                AccountId = cashTo, Id = cashTo, TenantId = Tenant, AccountCode = "1001",
                AccountName = "Cash B", AccountType = "Asset", IsPostingAllowed = true, CreatedBy = User.ToString()
            });
        db.AccountDefaults.Add(new AccountDefault
        {
            Id = Guid.NewGuid(),
            AccountDefaultId = Guid.NewGuid(),
            TenantId = Tenant,
            Purpose = AccountPurposes.AccountsReceivable,
            AccountId = arId,
            IsActive = true,
            CreatedBy = User.ToString()
        });
        var taxCodeId = Guid.NewGuid();
        db.TaxCodes.Add(new TaxCode
        {
            Id = taxCodeId,
            TaxCodeId = taxCodeId,
            TenantId = Tenant,
            Code = "GST",
            TaxCodeName = "GST",
            TaxRate = 10,
            TaxType = "Sales",
            AccountId = revenueId,
            CreatedBy = User.ToString()
        });
        var budgetId = Guid.NewGuid();
        db.Budgets.Add(new Budget
        {
            Id = budgetId,
            BudgetId = budgetId,
            TenantId = Tenant,
            AccountId = revenueId,
            BudgetName = "FY Budget",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            BudgetPeriodStart = new DateTime(2026, 1, 1),
            BudgetPeriodEnd = new DateTime(2026, 12, 31),
            BudgetAmount = 0,
            CurrencyCode = "USD",
            Status = "Active",
            CreatedBy = User.ToString()
        });
        await db.SaveChangesAsync();

        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        var gap = new FinanceGapService(db, accounting);

        var group = await gap.CreateTaxGroupAsync(new CreateTaxGroupDto
        {
            Code = "STD",
            Name = "Standard",
            TaxType = "Sales",
            Lines = { new CreateTaxGroupLineDto { TaxCodeId = taxCodeId, Sequence = 1 } }
        }, Tenant, User);
        Assert.Equal("STD", group.Code);
        Assert.Single(group.Lines);

        var line = await gap.AddBudgetLineAsync(new CreateBudgetLineDto
        {
            BudgetId = budgetId,
            PeriodStart = new DateTime(2026, 1, 1),
            PeriodEnd = new DateTime(2026, 1, 31),
            Amount = 1000
        }, Tenant, User);
        Assert.Equal(1000m, line.Amount);

        var memo = await gap.CreateCreditMemoAsync(new CreateCreditMemoDto
        {
            CustomerId = Customer,
            MemoDate = DateTime.UtcNow.Date,
            Lines =
            {
                new CreateMemoLineDto { Description = "Goodwill", Amount = 20, TaxAmount = 0, AccountId = revenueId }
            }
        }, Tenant, User);
        Assert.Equal("Draft", memo.Status);
        Assert.Equal(20m, memo.TotalAmount);
        var posted = await gap.PostCreditMemoAsync(memo.Id, Tenant, User);
        Assert.Equal("Posted", posted.Status);
        Assert.NotNull(posted.PostedJournalId);
        Assert.NotEqual(Guid.Empty, posted.PostedJournalId);

        var ft = await gap.CreateFundTransferAsync(new CreateFundTransferDto
        {
            TransferDate = DateTime.UtcNow.Date,
            Amount = 100,
            FromAccountId = cashFrom,
            ToAccountId = cashTo
        }, Tenant, User);
        Assert.Equal("Draft", ft.Status);
        var ftPosted = await gap.PostFundTransferAsync(ft.Id, Tenant, User);
        Assert.Equal("Posted", ftPosted.Status);
        Assert.NotNull(ftPosted.PostedJournalId);
        Assert.NotEqual(Guid.Empty, ftPosted.PostedJournalId);
    }

    [Fact]
    public async Task JournalReverse_SetsReversedFromJournalId()
    {
        await using var db = Db();
        var accountId = Guid.NewGuid();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            AccountId = accountId,
            Id = accountId,
            TenantId = Tenant,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = "Asset",
            IsPostingAllowed = true,
            CreatedBy = User.ToString()
        });
        var jId = Guid.NewGuid();
        db.Journals.Add(new Journal
        {
            Id = jId,
            JournalId = jId,
            TenantId = Tenant,
            JournalType = "Journal",
            JournalDate = DateTime.UtcNow.Date,
            JournalNumber = "JV-1",
            Status = "Posted",
            TotalDebit = 10,
            TotalCredit = 10,
            IsLocked = true,
            PostedAt = DateTime.UtcNow,
            PostedBy = User,
            CreatedBy = User.ToString()
        });
        db.JournalEntries.Add(new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            JournalId = jId,
            TenantId = Tenant,
            AccountId = accountId,
            Debit = 10,
            Credit = 0,
            EntryDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = User
        });
        db.JournalEntries.Add(new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            JournalId = jId,
            TenantId = Tenant,
            AccountId = accountId,
            Debit = 0,
            Credit = 10,
            EntryDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = User
        });
        await db.SaveChangesAsync();

        var journals = new ApplicationJournalRepositoryAdapter(db);
        Assert.True(await journals.ReverseJournalAsync(jId, Tenant, User, "test"));

        var rev = await db.Journals.AsNoTracking()
            .FirstOrDefaultAsync(j => j.ReversedFromJournalId == jId);
        Assert.NotNull(rev);
        Assert.Equal("Posted", rev!.Status);
    }
}
