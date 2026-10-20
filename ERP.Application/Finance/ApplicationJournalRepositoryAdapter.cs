using ERP.Application.Core.Settings;
using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class ApplicationJournalRepositoryAdapter : IJournalRepository
{
    private readonly ERPDbContext _db;
    private readonly ICoreSettingsService? _settings;
    private readonly IBudgetEnforcementService? _budgetEnforcement;

    public ApplicationJournalRepositoryAdapter(
        ERPDbContext db,
        ICoreSettingsService? settings = null,
        IBudgetEnforcementService? budgetEnforcement = null)
    {
        _db = db;
        _settings = settings;
        _budgetEnforcement = budgetEnforcement;
    }

    public async Task<Journal?> GetByIdAsync(Guid journalId, Guid tenantId)
    {
        var q = _db.Journals.Include(j => j.JournalEntries)
            .Where(j => j.JournalId == journalId || j.Id == journalId);
        if (tenantId != Guid.Empty) q = q.Where(j => j.TenantId == tenantId);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Journal>> GetAllAsync(
        Guid tenantId,
        string? journalType = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var q = _db.Journals.Include(j => j.JournalEntries).AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(j => j.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(journalType)) q = q.Where(j => j.JournalType == journalType);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(j => j.Status == status);
        if (fromDate.HasValue) q = q.Where(j => j.JournalDate >= fromDate);
        if (toDate.HasValue) q = q.Where(j => j.JournalDate <= toDate);
        return await q.OrderByDescending(j => j.JournalDate)
            .Skip(Math.Max(0, (pageNumber - 1) * pageSize))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(
        Guid tenantId,
        string? journalType = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = _db.Journals.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(j => j.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(journalType)) q = q.Where(j => j.JournalType == journalType);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(j => j.Status == status);
        if (fromDate.HasValue) q = q.Where(j => j.JournalDate >= fromDate);
        if (toDate.HasValue) q = q.Where(j => j.JournalDate <= toDate);
        return await q.CountAsync();
    }

    public async Task<Journal> AddAsync(Journal journal)
    {
        if (journal.Id == Guid.Empty) journal.Id = Guid.NewGuid();
        if (journal.JournalId == Guid.Empty) journal.JournalId = journal.Id;
        if (string.IsNullOrWhiteSpace(journal.JournalNumber))
        {
            var prefix = journal.JournalType switch
            {
                "CashPayment" => "CPV",
                "CashReceipt" => "CRV",
                "BankPayment" => "BPV",
                "BankReceipt" => "BRV",
                _ => "JV"
            };
            journal.JournalNumber = $"{prefix}-{DateTime.UtcNow:yyMM}-{journal.Id.ToString("N")[..6]}".ToUpperInvariant();
        }
        journal.CreatedAt = DateTime.UtcNow;
        _db.Journals.Add(journal);
        await _db.SaveChangesAsync();
        return journal;
    }

    public async Task<Journal> UpdateAsync(Journal journal)
    {
        journal.UpdatedAt = DateTime.UtcNow;
        _db.Journals.Update(journal);
        await _db.SaveChangesAsync();
        return journal;
    }

    public async Task<bool> DeleteAsync(Guid journalId, Guid tenantId)
    {
        var journal = await GetByIdAsync(journalId, tenantId);
        if (journal == null) return false;
        if (string.Equals(journal.Status, "Posted", StringComparison.OrdinalIgnoreCase)
            || journal.IsLocked)
            return false;

        var entries = await GetEntriesByJournalIdAsync(journal.JournalId, tenantId);
        _db.JournalEntries.RemoveRange(entries);
        _db.Journals.Remove(journal);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsLockedAsync(Guid journalId, Guid tenantId)
    {
        var journal = await GetByIdAsync(journalId, tenantId);
        return journal?.IsLocked == true
            || string.Equals(journal?.Status, "Posted", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> PostJournalAsync(Guid journalId, Guid tenantId, Guid postedBy, bool allowPostWithoutApprove = true)
    {
        var journal = await GetByIdAsync(journalId, tenantId);
        if (journal == null) return false;

        var isDraft = string.Equals(journal.Status, "Draft", StringComparison.OrdinalIgnoreCase);
        var isApproved = string.Equals(journal.Status, "Approved", StringComparison.OrdinalIgnoreCase);
        if (isApproved) { /* ok */ }
        else if (isDraft && allowPostWithoutApprove) { /* ok */ }
        else return false;

        if (!await IsJournalBalancedAsync(journal.JournalId, tenantId)) return false;

        if (_settings != null)
        {
            var requirePeriodRaw = await _settings.GetValueAsync(SettingsCatalog.RequireOpenPeriodKey);
            if (bool.TryParse(requirePeriodRaw, out var requirePeriod) && requirePeriod)
            {
                var d = journal.JournalDate.Date;
                var hasPeriods = await _db.AccountingPeriods.AnyAsync(p => p.TenantId == tenantId && p.IsActive);
                if (hasPeriods)
                {
                    var inOpen = await _db.AccountingPeriods.AnyAsync(p =>
                        p.TenantId == tenantId
                        && p.IsActive
                        && p.Status == "Open"
                        && p.StartDate <= d
                        && p.EndDate >= d);
                    if (!inOpen) return false;
                }
            }
        }

        if (_budgetEnforcement != null
            && await _budgetEnforcement.WouldExceedBudgetAsync(tenantId, journal.JournalId))
            return false;

        // When the tenant has Active fiscal years, journal date must fall in one.
        var hasActiveFy = await _db.FiscalYears.AnyAsync(f =>
            f.TenantId == tenantId && f.Status == "Active");
        if (hasActiveFy)
        {
            var d = journal.JournalDate.Date;
            var fy = await _db.FiscalYears
                .Where(f => f.TenantId == tenantId
                    && f.Status == "Active"
                    && f.StartDate <= d
                    && f.EndDate >= d)
                .OrderByDescending(f => f.StartDate)
                .FirstOrDefaultAsync();
            if (fy == null) return false;
            journal.FinancialYearId = fy.FiscalYearId;
        }

        journal.Status = "Posted";
        journal.PostedAt = DateTime.UtcNow;
        journal.PostedBy = postedBy;
        journal.IsLocked = true;
        journal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpostJournalAsync(Guid journalId, Guid tenantId, Guid unpostedBy)
    {
        var journal = await GetByIdAsync(journalId, tenantId);
        if (journal == null) return false;
        if (!string.Equals(journal.Status, "Posted", StringComparison.OrdinalIgnoreCase)) return false;

        journal.Status = "Draft";
        journal.PostedAt = null;
        journal.PostedBy = null;
        journal.ApprovedAt = null;
        journal.ApprovedBy = null;
        journal.FinancialYearId = null;
        journal.IsLocked = false;
        journal.UpdatedBy = unpostedBy.ToString();
        journal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReverseJournalAsync(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null)
    {
        var original = await GetByIdAsync(journalId, tenantId);
        if (original == null) return false;
        if (!string.Equals(original.Status, "Posted", StringComparison.OrdinalIgnoreCase)) return false;

        var entries = (await GetEntriesByJournalIdAsync(original.JournalId, tenantId)).ToList();
        if (entries.Count == 0) return false;

        var revId = Guid.NewGuid();
        var reversal = new Journal
        {
            Id = revId,
            JournalId = revId,
            TenantId = tenantId,
            JournalType = original.JournalType,
            ReferenceNo = $"REV-{original.JournalNumber}",
            JournalDate = DateTime.UtcNow.Date,
            Description = string.IsNullOrWhiteSpace(reversalReason)
                ? $"Reversal of {original.JournalNumber}"
                : $"Reversal of {original.JournalNumber}: {reversalReason}",
            CurrencyCode = original.CurrencyCode,
            ExchangeRate = original.ExchangeRate,
            TotalDebit = original.TotalCredit,
            TotalCredit = original.TotalDebit,
            Status = "Posted",
            JournalNumber = $"JV-REV-{DateTime.UtcNow:yyyyMMdd}-{revId:N}"[..24].ToUpperInvariant(),
            PostedAt = DateTime.UtcNow,
            PostedBy = reversedBy,
            IsLocked = true,
            CreatedBy = reversedBy.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Journals.Add(reversal);

        foreach (var e in entries)
        {
            _db.JournalEntries.Add(new JournalEntry
            {
                Id = Guid.NewGuid(),
                EntryId = Guid.NewGuid(),
                JournalId = revId,
                TenantId = tenantId,
                AccountId = e.AccountId,
                CostCenterId = e.CostCenterId,
                ProjectId = e.ProjectId,
                Debit = e.Credit,
                Credit = e.Debit,
                Narration = $"Reversal: {e.Narration}",
                EntityReferenceTable = "Journal",
                EntityReferenceId = original.JournalId,
                EntryDate = DateTime.UtcNow,
                Reference = original.JournalNumber,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = reversedBy
            });
        }

        original.Status = "Reversed";
        original.UpdatedAt = DateTime.UtcNow;
        original.UpdatedBy = reversedBy.ToString();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<JournalEntry>> GetEntriesByJournalIdAsync(Guid journalId, Guid tenantId)
    {
        var q = _db.JournalEntries.Where(e => e.JournalId == journalId);
        if (tenantId != Guid.Empty) q = q.Where(e => e.TenantId == tenantId);
        return await q.ToListAsync();
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(Guid entryId, Guid tenantId)
    {
        var q = _db.JournalEntries.Where(e => e.Id == entryId || e.EntryId == entryId);
        if (tenantId != Guid.Empty) q = q.Where(e => e.TenantId == tenantId);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<JournalEntry> AddEntryAsync(JournalEntry entry)
    {
        if (entry.Id == Guid.Empty) entry.Id = Guid.NewGuid();
        if (entry.EntryId == Guid.Empty) entry.EntryId = entry.Id;
        entry.CreatedAt = DateTime.UtcNow;
        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync();

        await RecalcTotalsAsync(entry.JournalId, entry.TenantId);
        return entry;
    }

    public async Task<JournalEntry> UpdateEntryAsync(JournalEntry entry)
    {
        _db.JournalEntries.Update(entry);
        await _db.SaveChangesAsync();
        await RecalcTotalsAsync(entry.JournalId, entry.TenantId);
        return entry;
    }

    public async Task<bool> DeleteEntryAsync(Guid entryId, Guid tenantId)
    {
        var entry = await GetEntryByIdAsync(entryId, tenantId);
        if (entry == null) return false;
        var journalId = entry.JournalId;
        _db.JournalEntries.Remove(entry);
        await _db.SaveChangesAsync();
        await RecalcTotalsAsync(journalId, tenantId);
        return true;
    }

    public async Task<decimal> GetTotalDebitAsync(Guid journalId, Guid tenantId)
        => (await GetEntriesByJournalIdAsync(journalId, tenantId)).Sum(e => e.Debit);

    public async Task<decimal> GetTotalCreditAsync(Guid journalId, Guid tenantId)
        => (await GetEntriesByJournalIdAsync(journalId, tenantId)).Sum(e => e.Credit);

    public async Task<bool> IsJournalBalancedAsync(Guid journalId, Guid tenantId)
    {
        var debit = await GetTotalDebitAsync(journalId, tenantId);
        var credit = await GetTotalCreditAsync(journalId, tenantId);
        return debit > 0 && Math.Round(debit, 2) == Math.Round(credit, 2);
    }

    public async Task<IEnumerable<JournalEntry>> GetEntriesByAccountAsync(
        Guid accountId, Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var q = from e in _db.JournalEntries
                join j in _db.Journals on e.JournalId equals j.JournalId
                where e.AccountId == accountId
                      && j.Status == "Posted"
                select e;
        if (tenantId != Guid.Empty) q = q.Where(e => e.TenantId == tenantId);
        if (fromDate.HasValue) q = q.Where(e => e.EntryDate >= fromDate);
        if (toDate.HasValue) q = q.Where(e => e.EntryDate <= toDate);
        return await q.ToListAsync();
    }

    public async Task<decimal> GetAccountBalanceAsync(Guid accountId, Guid tenantId, DateTime? asOfDate = null)
    {
        var entries = await GetEntriesByAccountAsync(accountId, tenantId, null, asOfDate);
        return entries.Sum(e => e.Debit - e.Credit);
    }

    public async Task<IEnumerable<JournalEntry>> GetEntriesByEntityReferenceAsync(
        string entityReferenceTable, Guid entityReferenceId, Guid tenantId)
    {
        var q = _db.JournalEntries.Where(e =>
            e.EntityReferenceTable == entityReferenceTable && e.EntityReferenceId == entityReferenceId);
        if (tenantId != Guid.Empty) q = q.Where(e => e.TenantId == tenantId);
        return await q.ToListAsync();
    }

    private async Task RecalcTotalsAsync(Guid journalId, Guid tenantId)
    {
        var journal = await GetByIdAsync(journalId, tenantId);
        if (journal == null) return;
        journal.TotalDebit = await GetTotalDebitAsync(journalId, tenantId);
        journal.TotalCredit = await GetTotalCreditAsync(journalId, tenantId);
        await _db.SaveChangesAsync();
    }
}
