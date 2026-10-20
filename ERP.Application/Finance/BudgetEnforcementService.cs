using ERP.Application.Core.Settings;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IBudgetEnforcementService
{
    Task<bool> WouldExceedBudgetAsync(Guid tenantId, Guid journalId);
}

public class BudgetEnforcementService : IBudgetEnforcementService
{
    private readonly ERPDbContext _db;
    private readonly ICoreSettingsService _settings;

    public BudgetEnforcementService(ERPDbContext db, ICoreSettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<bool> WouldExceedBudgetAsync(Guid tenantId, Guid journalId)
    {
        if (tenantId == Guid.Empty) return false;
        var raw = await _settings.GetValueAsync(SettingsCatalog.EnforceBudgetKey);
        if (!bool.TryParse(raw, out var enforce) || !enforce) return false;

        var journal = await _db.Journals.AsNoTracking()
            .FirstOrDefaultAsync(j => j.TenantId == tenantId && (j.JournalId == journalId || j.Id == journalId));
        if (journal == null) return false;

        var entries = await _db.JournalEntries.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.JournalId == journal.JournalId)
            .ToListAsync();
        if (entries.Count == 0) return false;

        var d = journal.JournalDate.Date;
        var budgets = await _db.Budgets.AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.Status == "Active")
            .Where(b =>
                (b.BudgetPeriodStart != default ? b.BudgetPeriodStart : b.StartDate) <= d
                && (b.BudgetPeriodEnd != default ? b.BudgetPeriodEnd : b.EndDate) >= d)
            .ToListAsync();
        if (budgets.Count == 0) return false;

        foreach (var budget in budgets)
        {
            var accountId = budget.AccountId;
            var newDebit = entries.Where(e => e.AccountId == accountId).Sum(e => e.Debit);
            if (newDebit <= 0) continue;

            var from = budget.BudgetPeriodStart != default ? budget.BudgetPeriodStart : budget.StartDate;
            var to = budget.BudgetPeriodEnd != default ? budget.BudgetPeriodEnd : budget.EndDate;
            var actual = await _db.JournalEntries.AsNoTracking()
                .Where(e => e.TenantId == tenantId && e.AccountId == accountId)
                .Join(_db.Journals.AsNoTracking().Where(j => j.TenantId == tenantId && j.Status == "Posted"),
                    e => e.JournalId, j => j.JournalId, (e, j) => new { e, j })
                .Where(x => x.j.JournalDate >= from && x.j.JournalDate <= to)
                .SumAsync(x => x.e.Debit);

            if (Math.Round(actual + newDebit, 2) > Math.Round(budget.BudgetAmount, 2))
                return true;
        }

        return false;
    }
}
