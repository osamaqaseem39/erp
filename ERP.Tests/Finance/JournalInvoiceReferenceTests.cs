using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class JournalInvoiceReferenceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Svc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db), db);

    [Fact]
    public async Task TypedVoucher_Persists_InvoiceNumber_And_EntityReference()
    {
        await using var db = Db();
        var cash = Guid.NewGuid();
        var expense = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = cash, AccountId = cash, TenantId = Tenant,
                AccountCode = "1111", AccountName = "Cash", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expense, AccountId = expense, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Rent", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var id = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            CashOrBankAccountId = cash,
            InvoiceNumber = " INV-1001 ",
            EntityReferenceTable = "Invoices",
            EntityReferenceId = invoiceId,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 40m }]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var dto = await svc.GetJournalByIdAsync(id, Tenant);
        Assert.NotNull(dto);
        Assert.Equal("INV-1001", dto!.InvoiceNumber);
        Assert.Equal("Invoices", dto.EntityReferenceTable);
        Assert.Equal(invoiceId, dto.EntityReferenceId);

        Assert.Equal(id, await svc.UpdateJournalAsync(new JournalUpdateDto
        {
            JournalId = id,
            InvoiceNumber = "INV-1001-B",
            EntityReferenceTable = "Bills",
            EntityReferenceId = Guid.NewGuid()
        }, Tenant, User));

        var updated = await svc.GetJournalByIdAsync(id, Tenant);
        Assert.Equal("INV-1001-B", updated!.InvoiceNumber);
        Assert.Equal("Bills", updated.EntityReferenceTable);
        Assert.NotEqual(invoiceId, updated.EntityReferenceId);
    }

    [Fact]
    public async Task Update_Can_Clear_InvoiceNumber()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.Journals.Add(new Journal
        {
            Id = id, JournalId = id, TenantId = Tenant,
            JournalType = "Journal", JournalNumber = "JV-1",
            JournalDate = DateTime.UtcNow.Date, Status = "Draft",
            InvoiceNumber = "X-1", EntityReferenceTable = "Invoices", EntityReferenceId = Guid.NewGuid()
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        await svc.UpdateJournalAsync(new JournalUpdateDto
        {
            JournalId = id,
            InvoiceNumber = "   "
        }, Tenant, User);

        var dto = await svc.GetJournalByIdAsync(id, Tenant);
        Assert.Null(dto!.InvoiceNumber);
    }
}
