using ERP.Application.Core.Settings;
using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class AccountingService : IAccountingService
{
    private const int DefaultMaxDepth = 4;

    private readonly IJournalRepository _journals;
    private readonly IChartOfAccountRepository _accounts;
    private readonly ERPDbContext? _db;
    private readonly ICoreSettingsService? _settings;

    public AccountingService(
        IJournalRepository journals,
        IChartOfAccountRepository accounts,
        ERPDbContext? db = null,
        ICoreSettingsService? settings = null)
    {
        _journals = journals;
        _accounts = accounts;
        _db = db;
        _settings = settings;
    }

    public async Task<Guid> AddChartOfAccountAsync(CreateChartOfAccountDto dto, Guid tenantId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.AccountCode) || string.IsNullOrWhiteSpace(dto.AccountName))
            return Guid.Empty;

        var code = dto.AccountCode.Trim();
        var existing = await _accounts.GetByAccountCodeAsync(code, tenantId);
        if (existing != null) return Guid.Empty;

        int level = 1;
        if (dto.ParentAccountId.HasValue && dto.ParentAccountId != Guid.Empty)
        {
            var parent = await _accounts.GetByIdAsync(dto.ParentAccountId.Value, tenantId);
            if (parent == null) return Guid.Empty;
            level = await ComputeLevelAsync(parent, tenantId) + 1;
            if (level > DefaultMaxDepth) return Guid.Empty;
        }

        var id = Guid.NewGuid();
        var entity = new ChartOfAccount
        {
            Id = id,
            AccountId = id,
            TenantId = tenantId,
            AccountCode = code,
            AccountName = dto.AccountName.Trim(),
            AccountType = string.IsNullOrWhiteSpace(dto.AccountType) ? "Asset" : dto.AccountType.Trim(),
            ParentAccountId = dto.ParentAccountId == Guid.Empty ? null : dto.ParentAccountId,
            CurrencyCode = dto.CurrencyCode ?? "USD",
            Remarks = dto.Remarks,
            IsActive = dto.IsActive,
            IsPostingAllowed = dto.IsPostingAllowed,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        var created = await _accounts.AddAsync(entity);
        return created.AccountId;
    }

    public async Task<Guid> UpdateChartOfAccountAsync(UpdateChartOfAccountDto dto, Guid tenantId, Guid userId)
    {
        var id = dto.AccountId ?? dto.Id;
        var entity = await _accounts.GetByIdAsync(id, tenantId);
        if (entity == null) return Guid.Empty;

        if (!string.IsNullOrWhiteSpace(dto.AccountCode)
            && !string.Equals(dto.AccountCode.Trim(), entity.AccountCode, StringComparison.OrdinalIgnoreCase))
        {
            var clash = await _accounts.GetByAccountCodeAsync(dto.AccountCode.Trim(), tenantId);
            if (clash != null && clash.AccountId != entity.AccountId) return Guid.Empty;
            entity.AccountCode = dto.AccountCode.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.AccountName)) entity.AccountName = dto.AccountName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.AccountType)) entity.AccountType = dto.AccountType.Trim();
        if (dto.Remarks != null) entity.Remarks = dto.Remarks;
        if (dto.ParentAccountId.HasValue)
        {
            if (dto.ParentAccountId == entity.AccountId) return Guid.Empty;
            if (dto.ParentAccountId != Guid.Empty)
            {
                var parent = await _accounts.GetByIdAsync(dto.ParentAccountId.Value, tenantId);
                if (parent == null) return Guid.Empty;
                var newLevel = await ComputeLevelAsync(parent, tenantId) + 1;
                if (newLevel > DefaultMaxDepth) return Guid.Empty;
            }
            entity.ParentAccountId = dto.ParentAccountId == Guid.Empty ? null : dto.ParentAccountId;
        }
        if (dto.CurrencyCode != null) entity.CurrencyCode = dto.CurrencyCode;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        if (dto.IsPostingAllowed.HasValue) entity.IsPostingAllowed = dto.IsPostingAllowed.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _accounts.UpdateAsync(entity);
        return entity.AccountId;
    }

    public async Task<bool> DeleteChartOfAccountAsync(Guid accountId, Guid tenantId)
        => await _accounts.DeleteAsync(accountId, tenantId);

    public async Task<ChartOfAccountDto?> GetChartOfAccountByIdAsync(Guid accountId, Guid tenantId)
    {
        var entity = await _accounts.GetByIdAsync(accountId, tenantId);
        if (entity == null) return null;
        var all = (await _accounts.GetAllAsync(tenantId)).ToList();
        return MapAccount(entity, all);
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetAllChartOfAccountsAsync(
        Guid tenantId, string? accountType = null, bool? isActive = null,
        int? level = null, Guid? parentAccountId = null, bool? postingOnly = null)
    {
        var entities = (await _accounts.GetAllAsync(tenantId, accountType, isActive)).ToList();
        var mapped = entities.Select(e => MapAccount(e, entities)).ToList();

        if (parentAccountId.HasValue)
            mapped = mapped.Where(a => a.ParentAccountId == parentAccountId).ToList();
        if (level.HasValue)
            mapped = mapped.Where(a => a.Level == level.Value).ToList();
        if (postingOnly == true)
            mapped = mapped.Where(a => a.IsPostingAllowed == true && a.HasChildren != true).ToList();

        return mapped.OrderBy(a => a.AccountCode);
    }

    public async Task<int> SeedDefaultChartOfAccountsAsync(Guid tenantId, Guid userId)
    {
        var existing = (await _accounts.GetAllAsync(tenantId)).ToList();
        if (existing.Count > 0) return 0;

        var seeds = new (string Code, string Name, string Type)[]
        {
            ("1000", "Assets", "Asset"),
            ("2000", "Liabilities", "Liability"),
            ("3000", "Equity", "Equity"),
            ("4000", "Income", "Revenue"),
            ("5000", "Expenses", "Expense"),
        };

        var count = 0;
        foreach (var (code, name, type) in seeds)
        {
            var id = await AddChartOfAccountAsync(new CreateChartOfAccountDto
            {
                AccountCode = code,
                AccountName = name,
                AccountType = type,
                IsPostingAllowed = false,
                IsActive = true
            }, tenantId, userId);
            if (id != Guid.Empty) count++;
        }
        return count;
    }

    public async Task<VoucherDefaultsDto> GetVoucherDefaultsAsync(Guid tenantId)
    {
        var result = new VoucherDefaultsDto();
        if (_settings == null) return result;

        var cashRaw = await _settings.GetValueAsync(SettingsCatalog.LastCashOrBankAccountIdKey);
        if (Guid.TryParse(cashRaw, out var cashId) && cashId != Guid.Empty)
        {
            var account = await _accounts.GetByIdAsync(cashId, tenantId);
            if (account is { IsActive: true, IsPostingAllowed: true })
            {
                result.CashOrBankAccountId = cashId;
                result.CashOrBankAccountCode = account.AccountCode;
                result.CashOrBankAccountName = account.AccountName;
            }
        }

        var ccRaw = await _settings.GetValueAsync(SettingsCatalog.LastCostCenterIdKey);
        if (Guid.TryParse(ccRaw, out var ccId) && ccId != Guid.Empty)
        {
            if (tenantId != Guid.Empty && (_db == null || await _db.CostCenters.AsNoTracking()
                    .AnyAsync(c => c.CostCenterId == ccId && c.IsActive && c.TenantId == tenantId)))
                result.CostCenterId = ccId;
        }

        return result;
    }

    public async Task<Guid> AddJournalAsync(JournalCreateDto dto, Guid tenantId, Guid userId)
    {
        var journalType = string.IsNullOrWhiteSpace(dto.JournalType) ? "Adjustment" : dto.JournalType.Trim();
        var isPayment = IsPaymentVoucher(journalType);
        var isReceipt = IsReceiptVoucher(journalType);
        var isTypedCashBank = isPayment || isReceipt;

        var offsetLines = ResolveOffsetLines(dto);
        if (isTypedCashBank && offsetLines.Count > 0)
        {
            VoucherDefaultsDto? defaults = null;
            if (dto.CashOrBankAccountId == null || dto.CashOrBankAccountId == Guid.Empty)
            {
                defaults = await GetVoucherDefaultsAsync(tenantId);
                if (defaults.CashOrBankAccountId.HasValue)
                    dto.CashOrBankAccountId = defaults.CashOrBankAccountId;
            }

            if (dto.CashOrBankAccountId == null || dto.CashOrBankAccountId == Guid.Empty)
                return Guid.Empty;
            if (offsetLines.All(l => l.AccountId == Guid.Empty || l.Amount <= 0))
                return Guid.Empty;

            // Apply last cost center to offset lines that omit one.
            defaults ??= await GetVoucherDefaultsAsync(tenantId);
            if (defaults.CostCenterId.HasValue)
            {
                foreach (var line in offsetLines.Where(l => l.CostCenterId == null || l.CostCenterId == Guid.Empty))
                    line.CostCenterId = defaults.CostCenterId;
            }
        }

        var id = Guid.NewGuid();
        var entity = new Journal
        {
            Id = id,
            JournalId = id,
            TenantId = tenantId,
            JournalType = journalType,
            ReferenceNo = dto.ReferenceNo,
            JournalDate = dto.JournalDate == default ? DateTime.UtcNow.Date : dto.JournalDate,
            ReferenceDate = dto.ReferenceDate,
            Description = dto.Description,
            CurrencyCode = dto.CurrencyCode ?? "USD",
            ExchangeRate = dto.ExchangeRate ?? 1m,
            ChequeNumber = dto.ChequeNumber,
            ChequeStatus = string.IsNullOrWhiteSpace(dto.ChequeNumber) ? null : "Outstanding",
            ChequeStatusAt = string.IsNullOrWhiteSpace(dto.ChequeNumber) ? null : DateTime.UtcNow,
            PaidBy = dto.PaidBy,
            ReceivedBy = dto.ReceivedBy,
            CashOrBankAccountId = dto.CashOrBankAccountId,
            InvoiceNumber = string.IsNullOrWhiteSpace(dto.InvoiceNumber) ? null : dto.InvoiceNumber.Trim(),
            EntityReferenceTable = string.IsNullOrWhiteSpace(dto.EntityReferenceTable) ? null : dto.EntityReferenceTable.Trim(),
            EntityReferenceId = dto.EntityReferenceId,
            Status = "Draft",
            JournalNumber = BuildJournalNumber(journalType, id),
            IsLocked = false,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        var created = await _journals.AddAsync(entity);

        if (isTypedCashBank && offsetLines.Count > 0)
        {
            decimal total = 0;
            foreach (var line in offsetLines.Where(l => l.AccountId != Guid.Empty && l.Amount > 0))
            {
                // Payment: Dr expense/personal; Receipt: Cr income/AR
                var debit = isPayment ? line.Amount : 0m;
                var credit = isReceipt ? line.Amount : 0m;
                await _journals.AddEntryAsync(new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    EntryId = Guid.NewGuid(),
                    JournalId = created.JournalId,
                    TenantId = tenantId,
                    AccountId = line.AccountId,
                    CostCenterId = line.CostCenterId,
                    Debit = debit,
                    Credit = credit,
                    Narration = line.Narration,
                    EntryDate = created.JournalDate,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
                total += line.Amount;
            }

            // Payment: Cr cash/bank; Receipt: Dr cash/bank
            await _journals.AddEntryAsync(new JournalEntry
            {
                Id = Guid.NewGuid(),
                EntryId = Guid.NewGuid(),
                JournalId = created.JournalId,
                TenantId = tenantId,
                AccountId = dto.CashOrBankAccountId!.Value,
                Debit = isReceipt ? total : 0m,
                Credit = isPayment ? total : 0m,
                Narration = dto.Description ?? journalType,
                EntryDate = created.JournalDate,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });

            await RememberVoucherDefaultsAsync(dto.CashOrBankAccountId, offsetLines);
        }

        return created.JournalId;
    }

    public async Task<Guid> UnpostJournalAsync(Guid journalId, Guid tenantId, Guid unpostedBy)
    {
        var ok = await _journals.UnpostJournalAsync(journalId, tenantId, unpostedBy);
        return ok ? journalId : Guid.Empty;
    }

    public async Task<Guid> UpdateJournalAsync(JournalUpdateDto dto, Guid tenantId, Guid userId)
    {
        var id = dto.JournalId ?? dto.Id;
        var entity = await _journals.GetByIdAsync(id, tenantId);
        if (entity == null) return Guid.Empty;
        if (entity.IsLocked
            || string.Equals(entity.Status, "Posted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Reversed", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;

        if (!string.IsNullOrWhiteSpace(dto.JournalType)) entity.JournalType = dto.JournalType;
        if (dto.ReferenceNo != null) entity.ReferenceNo = dto.ReferenceNo;
        if (dto.JournalDate.HasValue) entity.JournalDate = dto.JournalDate.Value;
        if (dto.ReferenceDate.HasValue) entity.ReferenceDate = dto.ReferenceDate;
        if (dto.Description != null) entity.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.CurrencyCode)) entity.CurrencyCode = dto.CurrencyCode;
        if (dto.ExchangeRate.HasValue) entity.ExchangeRate = dto.ExchangeRate;
        if (dto.ChequeNumber != null)
        {
            entity.ChequeNumber = dto.ChequeNumber;
            if (!string.IsNullOrWhiteSpace(dto.ChequeNumber) && string.IsNullOrWhiteSpace(entity.ChequeStatus))
            {
                entity.ChequeStatus = "Outstanding";
                entity.ChequeStatusAt = DateTime.UtcNow;
            }
            if (string.IsNullOrWhiteSpace(dto.ChequeNumber))
            {
                entity.ChequeStatus = null;
                entity.ChequeStatusAt = null;
                entity.ChequeStatusNotes = null;
            }
        }
        if (dto.PaidBy != null) entity.PaidBy = dto.PaidBy;
        if (dto.ReceivedBy != null) entity.ReceivedBy = dto.ReceivedBy;
        if (dto.CashOrBankAccountId.HasValue) entity.CashOrBankAccountId = dto.CashOrBankAccountId;
        if (dto.InvoiceNumber != null)
            entity.InvoiceNumber = string.IsNullOrWhiteSpace(dto.InvoiceNumber) ? null : dto.InvoiceNumber.Trim();
        if (dto.EntityReferenceTable != null)
            entity.EntityReferenceTable = string.IsNullOrWhiteSpace(dto.EntityReferenceTable) ? null : dto.EntityReferenceTable.Trim();
        if (dto.EntityReferenceId.HasValue) entity.EntityReferenceId = dto.EntityReferenceId;
        // Status intentionally ignored — use Approve/Post/Reject
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _journals.UpdateAsync(entity);
        return entity.JournalId;
    }

    public async Task<Guid> ApproveJournalAsync(Guid journalId, Guid tenantId, Guid approvedBy)
    {
        var journal = await _journals.GetByIdAsync(journalId, tenantId);
        if (journal == null) return Guid.Empty;
        if (!string.Equals(journal.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;
        if (!await _journals.IsJournalBalancedAsync(journal.JournalId, tenantId))
            return Guid.Empty;

        if (!string.IsNullOrEmpty(journal.CreatedBy)
            && Guid.TryParse(journal.CreatedBy, out var creator)
            && creator == approvedBy
            && creator != Guid.Empty)
            return Guid.Empty; // approver ≠ creator

        journal.Status = "Approved";
        journal.ApprovedAt = DateTime.UtcNow;
        journal.ApprovedBy = approvedBy;
        journal.UpdatedBy = approvedBy.ToString();
        journal.UpdatedAt = DateTime.UtcNow;
        await _journals.UpdateAsync(journal);
        return journal.JournalId;
    }

    public async Task<Guid> RejectJournalAsync(Guid journalId, Guid tenantId, Guid rejectedBy, string? reason = null)
    {
        var journal = await _journals.GetByIdAsync(journalId, tenantId);
        if (journal == null) return Guid.Empty;
        if (!string.Equals(journal.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;

        journal.Status = "Draft";
        journal.ApprovedAt = null;
        journal.ApprovedBy = null;
        if (!string.IsNullOrWhiteSpace(reason))
            journal.Description = string.IsNullOrWhiteSpace(journal.Description)
                ? $"Rejected: {reason}"
                : $"{journal.Description} | Rejected: {reason}";
        journal.UpdatedBy = rejectedBy.ToString();
        journal.UpdatedAt = DateTime.UtcNow;
        await _journals.UpdateAsync(journal);
        return journal.JournalId;
    }

    public async Task<Guid> PostJournalAsync(Guid journalId, Guid tenantId, Guid postedBy, bool allowPostWithoutApprove = true)
    {
        var ok = await _journals.PostJournalAsync(journalId, tenantId, postedBy, allowPostWithoutApprove);
        return ok ? journalId : Guid.Empty;
    }

    public async Task<Guid> ReverseJournalAsync(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null)
    {
        var ok = await _journals.ReverseJournalAsync(journalId, tenantId, reversedBy, reversalReason);
        return ok ? journalId : Guid.Empty;
    }

    public async Task<JournalDto?> GetJournalByIdAsync(Guid journalId, Guid tenantId)
    {
        var entity = await _journals.GetByIdAsync(journalId, tenantId);
        if (entity == null) return null;
        var entries = await _journals.GetEntriesByJournalIdAsync(entity.JournalId, tenantId);
        return MapJournal(entity, entries);
    }

    public async Task<(IEnumerable<JournalDto> Journals, int TotalCount)> SearchJournalsAsync(JournalQuery query)
    {
        var items = await _journals.GetAllAsync(
            query.TenantId, query.JournalType, query.Status, query.FromDate, query.ToDate,
            query.PageNumber, query.PageSize);
        var count = await _journals.GetCountAsync(
            query.TenantId, query.JournalType, query.Status, query.FromDate, query.ToDate);
        return (items.Select(j => MapJournal(j, j.JournalEntries)), count);
    }

    public async Task<Guid> AddJournalEntryAsync(CreateJournalEntryDto dto, Guid journalId, Guid tenantId, Guid userId)
    {
        var journal = await _journals.GetByIdAsync(journalId, tenantId);
        if (journal == null) return Guid.Empty;
        if (journal.IsLocked || !string.Equals(journal.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;
        if (dto.AccountId == Guid.Empty) return Guid.Empty;
        if (dto.Debit < 0 || dto.Credit < 0) return Guid.Empty;
        if (dto.Debit == 0 && dto.Credit == 0) return Guid.Empty;
        if (dto.Debit > 0 && dto.Credit > 0) return Guid.Empty;

        var entryId = Guid.NewGuid();
        var entry = new JournalEntry
        {
            Id = entryId,
            EntryId = entryId,
            JournalId = journal.JournalId,
            TenantId = tenantId,
            AccountId = dto.AccountId,
            CostCenterId = dto.CostCenterId,
            ProjectId = dto.ProjectId,
            Debit = dto.Debit,
            Credit = dto.Credit,
            Narration = dto.Narration,
            EntityReferenceTable = dto.EntityReferenceTable,
            EntityReferenceId = dto.EntityReferenceId,
            EntryDate = journal.JournalDate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _journals.AddEntryAsync(entry);
        return created.EntryId;
    }

    public async Task<Guid> UpdateJournalEntryAsync(UpdateJournalEntryDto dto, Guid tenantId, Guid userId)
    {
        var entryId = dto.EntryId ?? dto.Id;
        var entry = await _journals.GetEntryByIdAsync(entryId, tenantId);
        if (entry == null) return Guid.Empty;

        var journal = await _journals.GetByIdAsync(entry.JournalId, tenantId);
        if (journal == null || journal.IsLocked
            || !string.Equals(journal.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;

        if (dto.AccountId.HasValue) entry.AccountId = dto.AccountId.Value;
        if (dto.CostCenterId.HasValue) entry.CostCenterId = dto.CostCenterId;
        if (dto.ProjectId.HasValue) entry.ProjectId = dto.ProjectId;
        if (dto.Debit.HasValue) entry.Debit = dto.Debit.Value;
        if (dto.Credit.HasValue) entry.Credit = dto.Credit.Value;
        if (dto.Narration != null) entry.Narration = dto.Narration;
        if (dto.EntityReferenceTable != null) entry.EntityReferenceTable = dto.EntityReferenceTable;
        if (dto.EntityReferenceId.HasValue) entry.EntityReferenceId = dto.EntityReferenceId;

        if (entry.Debit > 0 && entry.Credit > 0) return Guid.Empty;
        if (entry.Debit == 0 && entry.Credit == 0) return Guid.Empty;

        await _journals.UpdateEntryAsync(entry);
        return entry.EntryId;
    }

    public async Task<bool> DeleteJournalEntryAsync(Guid entryId, Guid tenantId)
    {
        var entry = await _journals.GetEntryByIdAsync(entryId, tenantId);
        if (entry == null) return false;
        var journal = await _journals.GetByIdAsync(entry.JournalId, tenantId);
        if (journal == null || journal.IsLocked
            || !string.Equals(journal.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _journals.DeleteEntryAsync(entryId, tenantId);
    }

    public async Task<IEnumerable<JournalEntryDto>> GetJournalEntriesByJournalIdAsync(Guid journalId, Guid tenantId)
    {
        var entries = await _journals.GetEntriesByJournalIdAsync(journalId, tenantId);
        return entries.Select(MapEntry);
    }

    public async Task<TrialBalanceReportDto> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        var posted = await LoadPostedEntriesAsync(tenantId, fromDate, toDate);
        var accounts = (await _accounts.GetAllAsync(tenantId)).ToList();
        var lines = BuildTbLines(posted.Select(p => p.Entry), accounts)
            .OrderBy(l => l.AccountCode)
            .ToList();

        return new TrialBalanceReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            CurrencyCode = currencyCode ?? "USD",
            Lines = lines,
            TotalDebit = Math.Round(lines.Sum(l => l.Debit), 2),
            TotalCredit = Math.Round(lines.Sum(l => l.Credit), 2)
        };
    }

    public async Task<RunningTrialBalanceReportDto> GetRunningTrialBalanceAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        var allThrough = await LoadPostedEntriesAsync(tenantId, null, toDate);
        var accounts = (await _accounts.GetAllAsync(tenantId)).ToList();
        var accountIds = allThrough.Select(p => p.Entry.AccountId).Distinct().ToList();

        var lines = new List<RunningTrialBalanceLineDto>();
        foreach (var accountId in accountIds)
        {
            var account = accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null) continue;

            var opening = allThrough.Where(p => p.Entry.AccountId == accountId && p.Journal.JournalDate < fromDate).Select(p => p.Entry).ToList();
            var period = allThrough.Where(p => p.Entry.AccountId == accountId && p.Journal.JournalDate >= fromDate && p.Journal.JournalDate <= toDate).Select(p => p.Entry).ToList();

            var od = Math.Round(opening.Sum(e => e.Debit), 2);
            var oc = Math.Round(opening.Sum(e => e.Credit), 2);
            var pd = Math.Round(period.Sum(e => e.Debit), 2);
            var pc = Math.Round(period.Sum(e => e.Credit), 2);

            lines.Add(new RunningTrialBalanceLineDto
            {
                AccountId = accountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                Level = ComputeLevelFromList(account, accounts),
                OpeningDebit = od,
                OpeningCredit = oc,
                PeriodDebit = pd,
                PeriodCredit = pc,
                ClosingDebit = Math.Round(od + pd, 2),
                ClosingCredit = Math.Round(oc + pc, 2),
                OpeningBalance = Math.Round(od - oc, 2),
                ClosingBalance = Math.Round(od + pd - (oc + pc), 2)
            });
        }

        return new RunningTrialBalanceReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            CurrencyCode = currencyCode ?? "USD",
            Lines = lines.OrderBy(l => l.AccountCode).ToList()
        };
    }

    public async Task<LedgerReportDto> GetGeneralLedgerBalanceAsync(
        Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate, bool postingOnly = false)
    {
        var account = await _accounts.GetByIdAsync(accountId, tenantId);
        if (postingOnly && account != null && !account.IsPostingAllowed)
        {
            return new LedgerReportDto
            {
                AccountId = accountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

        var allThrough = await LoadPostedEntriesAsync(tenantId, null, toDate);
        var openingEntries = allThrough.Where(p => p.Entry.AccountId == accountId && p.Journal.JournalDate < fromDate).ToList();
        var period = allThrough
            .Where(p => p.Entry.AccountId == accountId && p.Journal.JournalDate >= fromDate && p.Journal.JournalDate <= toDate)
            .OrderBy(p => p.Journal.JournalDate)
            .ThenBy(p => p.Entry.CreatedAt)
            .ToList();

        var opening = Math.Round(openingEntries.Sum(p => p.Entry.Debit - p.Entry.Credit), 2);
        var running = opening;
        var lines = new List<LedgerLineDto>();
        foreach (var (journal, entry) in period)
        {
            running = Math.Round(running + entry.Debit - entry.Credit, 2);
            lines.Add(new LedgerLineDto
            {
                EntryId = entry.EntryId,
                JournalId = entry.JournalId,
                JournalNumber = journal.JournalNumber,
                JournalType = journal.JournalType,
                EntryDate = journal.JournalDate,
                Debit = entry.Debit,
                Credit = entry.Credit,
                Narration = entry.Narration,
                RunningBalance = running
            });
        }

        return new LedgerReportDto
        {
            AccountId = accountId,
            AccountCode = account?.AccountCode,
            AccountName = account?.AccountName,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = opening,
            Entries = lines,
            ClosingBalance = running
        };
    }

    public async Task<ProfitAndLossReportDto> GetProfitAndLossAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        var posted = await LoadPostedEntriesAsync(tenantId, fromDate, toDate);
        var accounts = (await _accounts.GetAllAsync(tenantId)).ToList();
        var byAccount = posted.GroupBy(p => p.Entry.AccountId);

        var revenue = new List<ProfitAndLossLineDto>();
        var expenses = new List<ProfitAndLossLineDto>();

        foreach (var g in byAccount)
        {
            var account = accounts.FirstOrDefault(a => a.AccountId == g.Key);
            if (account == null) continue;
            var type = account.AccountType?.Trim() ?? string.Empty;
            var debit = g.Sum(x => x.Entry.Debit);
            var credit = g.Sum(x => x.Entry.Credit);

            if (IsRevenueType(type))
            {
                var amount = Math.Round(credit - debit, 2);
                if (amount == 0) continue;
                revenue.Add(new ProfitAndLossLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    AccountType = account.AccountType,
                    Amount = amount
                });
            }
            else if (IsExpenseType(type))
            {
                var amount = Math.Round(debit - credit, 2);
                if (amount == 0) continue;
                expenses.Add(new ProfitAndLossLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    AccountType = account.AccountType,
                    Amount = amount
                });
            }
        }

        var totalRevenue = Math.Round(revenue.Sum(r => r.Amount), 2);
        var totalExpenses = Math.Round(expenses.Sum(e => e.Amount), 2);
        return new ProfitAndLossReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            CurrencyCode = currencyCode ?? "USD",
            Revenue = revenue.OrderBy(r => r.AccountCode).ToList(),
            Expenses = expenses.OrderBy(e => e.AccountCode).ToList(),
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetIncome = Math.Round(totalRevenue - totalExpenses, 2)
        };
    }

    public async Task<BalanceSheetReportDto> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null)
    {
        var posted = await LoadPostedEntriesAsync(tenantId, null, asOfDate);
        var accounts = (await _accounts.GetAllAsync(tenantId)).ToList();
        var tbLines = BuildTbLines(posted.Select(p => p.Entry), accounts);

        var assets = new BalanceSheetSectionDto { Section = "Assets" };
        var liabilities = new BalanceSheetSectionDto { Section = "Liabilities" };
        var equity = new BalanceSheetSectionDto { Section = "Equity" };

        foreach (var line in tbLines.OrderBy(l => l.AccountCode))
        {
            if (IsAssetType(line.AccountType))
            {
                var signed = new TrialBalanceLineDto
                {
                    AccountId = line.AccountId,
                    AccountCode = line.AccountCode,
                    AccountName = line.AccountName,
                    AccountType = line.AccountType,
                    Level = line.Level,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Balance = Math.Round(line.Debit - line.Credit, 2)
                };
                if (signed.Balance == 0) continue;
                assets.Lines.Add(signed);
            }
            else if (IsLiabilityType(line.AccountType))
            {
                var signed = CloneWithBalance(line, Math.Round(line.Credit - line.Debit, 2));
                if (signed.Balance == 0) continue;
                liabilities.Lines.Add(signed);
            }
            else if (IsEquityType(line.AccountType))
            {
                var signed = CloneWithBalance(line, Math.Round(line.Credit - line.Debit, 2));
                if (signed.Balance == 0) continue;
                equity.Lines.Add(signed);
            }
        }

        // Net income through as-of (all P&L activity) plugs into equity
        var pnl = await GetProfitAndLossAsync(tenantId, DateTime.MinValue, asOfDate, currencyCode);
        if (pnl.NetIncome != 0)
        {
            equity.Lines.Add(new TrialBalanceLineDto
            {
                AccountId = Guid.Empty,
                AccountCode = "NI",
                AccountName = "Net Income (current)",
                AccountType = "Equity",
                Level = 1,
                Balance = pnl.NetIncome,
                Credit = pnl.NetIncome > 0 ? pnl.NetIncome : 0,
                Debit = pnl.NetIncome < 0 ? Math.Abs(pnl.NetIncome) : 0
            });
        }

        assets.Total = Math.Round(assets.Lines.Sum(l => l.Balance), 2);
        liabilities.Total = Math.Round(liabilities.Lines.Sum(l => l.Balance), 2);
        equity.Total = Math.Round(equity.Lines.Sum(l => l.Balance), 2);

        return new BalanceSheetReportDto
        {
            AsOfDate = asOfDate,
            CurrencyCode = currencyCode ?? "USD",
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity,
            NetIncomePlug = pnl.NetIncome,
            TotalAssets = assets.Total,
            TotalLiabilitiesAndEquity = Math.Round(liabilities.Total + equity.Total, 2)
        };
    }

    public async Task<VoucherReportDto> GetVoucherReportAsync(
        Guid tenantId, DateTime? fromDate, DateTime? toDate, string? journalType = null, string? status = null)
    {
        var journals = (await _journals.GetAllAsync(
            tenantId, journalType, status, fromDate, toDate, pageNumber: 1, pageSize: 10_000)).ToList();

        var lines = journals.Select(j => new VoucherReportLineDto
        {
            JournalId = j.JournalId,
            JournalNumber = j.JournalNumber,
            JournalType = j.JournalType,
            JournalDate = j.JournalDate,
            Status = j.Status,
            Description = j.Description,
            TotalDebit = j.TotalDebit,
            TotalCredit = j.TotalCredit
        }).OrderByDescending(l => l.JournalDate).ToList();

        return new VoucherReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            JournalType = journalType,
            Status = status,
            Lines = lines,
            TotalDebit = Math.Round(lines.Sum(l => l.TotalDebit), 2),
            TotalCredit = Math.Round(lines.Sum(l => l.TotalCredit), 2),
            Count = lines.Count
        };
    }

    public async Task<ChartOfAccountReportDto> GetChartOfAccountReportAsync(Guid tenantId, bool includeDetailAccounts = true)
    {
        var all = (await _accounts.GetAllAsync(tenantId)).Where(a => a.IsActive).ToList();
        if (!includeDetailAccounts)
            all = all.Where(a => !a.IsPostingAllowed).ToList();

        var dtos = all.Select(a => MapAccount(a, all)).OrderBy(a => a.AccountCode).ToList();
        var lookup = dtos.ToDictionary(a => a.AccountId!.Value, a => new ChartOfAccountReportNodeDto
        {
            AccountId = a.AccountId!.Value,
            AccountCode = a.AccountCode ?? string.Empty,
            AccountName = a.AccountName ?? string.Empty,
            AccountType = a.AccountType ?? string.Empty,
            Level = a.Level ?? 1,
            IsPostingAllowed = a.IsPostingAllowed ?? false,
            Remarks = a.Remarks
        });

        var roots = new List<ChartOfAccountReportNodeDto>();
        foreach (var a in dtos)
        {
            var node = lookup[a.AccountId!.Value];
            if (a.ParentAccountId.HasValue && lookup.TryGetValue(a.ParentAccountId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return new ChartOfAccountReportDto
        {
            IncludeDetailAccounts = includeDetailAccounts,
            Roots = roots
        };
    }

    public async Task<DailyFinancialReportDto> GetDailyFinancialAsync(
        Guid tenantId, DateTime date, string? currencyCode = null)
    {
        var day = date.Date;
        var journals = (await _journals.GetAllAsync(
            tenantId, status: "Posted", fromDate: day, toDate: day, pageNumber: 1, pageSize: 10_000)).ToList();

        var byType = journals
            .GroupBy(j => j.JournalType)
            .Select(g => new DailyFinancialTypeTotalDto
            {
                JournalType = g.Key,
                Count = g.Count(),
                TotalDebit = Math.Round(g.Sum(x => x.TotalDebit), 2),
                TotalCredit = Math.Round(g.Sum(x => x.TotalCredit), 2)
            })
            .OrderBy(x => x.JournalType)
            .ToList();

        decimal SumType(string type) =>
            Math.Round(journals.Where(j => string.Equals(j.JournalType, type, StringComparison.OrdinalIgnoreCase))
                .Sum(j => j.TotalDebit), 2);

        return new DailyFinancialReportDto
        {
            Date = day,
            CurrencyCode = currencyCode ?? "USD",
            ByType = byType,
            CashPayments = SumType("CashPayment"),
            CashReceipts = SumType("CashReceipt"),
            BankPayments = SumType("BankPayment"),
            BankReceipts = SumType("BankReceipt"),
            VoucherCount = journals.Count,
            TotalDebit = Math.Round(journals.Sum(j => j.TotalDebit), 2),
            TotalCredit = Math.Round(journals.Sum(j => j.TotalCredit), 2)
        };
    }

    public async Task<CashBankVoucherSummaryReportDto> GetCashBankVoucherSummaryAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var types = new[] { "CashPayment", "CashReceipt", "BankPayment", "BankReceipt" };
        var journals = (await _journals.GetAllAsync(
            tenantId, status: "Posted", fromDate: fromDate, toDate: toDate, pageNumber: 1, pageSize: 50_000))
            .Where(j => types.Contains(j.JournalType, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var lines = types.Select(t =>
        {
            var set = journals.Where(j => string.Equals(j.JournalType, t, StringComparison.OrdinalIgnoreCase)).ToList();
            return new CashBankVoucherSummaryLineDto
            {
                JournalType = t,
                Count = set.Count,
                TotalDebit = Math.Round(set.Sum(x => x.TotalDebit), 2),
                TotalCredit = Math.Round(set.Sum(x => x.TotalCredit), 2),
                TotalAmount = Math.Round(set.Sum(x => x.TotalDebit), 2)
            };
        }).ToList();

        return new CashBankVoucherSummaryReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Lines = lines
        };
    }

    public async Task<CashBankLedgerSummaryReportDto> GetCashBankLedgerSummaryAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var accounts = (await _accounts.GetAllAsync(tenantId, isActive: true))
            .Where(a => a.IsPostingAllowed && string.Equals(a.AccountType, "Asset", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Prefer Cash / BankClearing defaults when mapped
        var cashIds = new HashSet<Guid>();
        // Include accounts used as CashOrBankAccountId on posted cash/bank vouchers in range + opening
        var typed = (await _journals.GetAllAsync(
            tenantId, status: "Posted", fromDate: null, toDate: toDate, pageNumber: 1, pageSize: 50_000))
            .Where(j => IsPaymentVoucher(j.JournalType) || IsReceiptVoucher(j.JournalType))
            .ToList();
        foreach (var j in typed.Where(j => j.CashOrBankAccountId.HasValue))
            cashIds.Add(j.CashOrBankAccountId!.Value);

        if (cashIds.Count == 0)
        {
            // Fallback: asset posting accounts whose code/name suggests cash/bank
            foreach (var a in accounts.Where(a =>
                         a.AccountName.Contains("cash", StringComparison.OrdinalIgnoreCase)
                         || a.AccountName.Contains("bank", StringComparison.OrdinalIgnoreCase)
                         || a.AccountCode.Contains("cash", StringComparison.OrdinalIgnoreCase)
                         || a.AccountCode.Contains("bank", StringComparison.OrdinalIgnoreCase)))
                cashIds.Add(a.AccountId);
        }

        var lines = new List<CashBankLedgerSummaryLineDto>();
        foreach (var accountId in cashIds)
        {
            var account = accounts.FirstOrDefault(a => a.AccountId == accountId)
                          ?? (await _accounts.GetByIdAsync(accountId, tenantId));
            if (account == null) continue;
            var ledger = await GetGeneralLedgerBalanceAsync(accountId, tenantId, fromDate, toDate);
            lines.Add(new CashBankLedgerSummaryLineDto
            {
                AccountId = accountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                OpeningBalance = ledger.OpeningBalance,
                PeriodDebit = Math.Round(ledger.Entries.Sum(e => e.Debit), 2),
                PeriodCredit = Math.Round(ledger.Entries.Sum(e => e.Credit), 2),
                ClosingBalance = ledger.ClosingBalance
            });
        }

        return new CashBankLedgerSummaryReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Lines = lines.OrderBy(l => l.AccountCode).ToList()
        };
    }

    public async Task<VoucherTypeMonthlyReportDto> GetVoucherTypeMonthlySummaryAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var journals = (await _journals.GetAllAsync(
            tenantId, status: "Posted", fromDate: fromDate, toDate: toDate, pageNumber: 1, pageSize: 50_000)).ToList();

        var lines = journals
            .GroupBy(j => new { j.JournalDate.Year, j.JournalDate.Month, j.JournalType })
            .Select(g => new VoucherTypeMonthlyLineDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Period = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                JournalType = g.Key.JournalType,
                Count = g.Count(),
                TotalDebit = Math.Round(g.Sum(x => x.TotalDebit), 2),
                TotalCredit = Math.Round(g.Sum(x => x.TotalCredit), 2)
            })
            .OrderBy(l => l.Period)
            .ThenBy(l => l.JournalType)
            .ToList();

        return new VoucherTypeMonthlyReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Lines = lines
        };
    }

    public async Task<CashFlowReportDto> GetCashFlowAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        var types = new[] { "CashPayment", "CashReceipt", "BankPayment", "BankReceipt" };
        var inflows = new List<CashFlowLineDto>();
        var outflows = new List<CashFlowLineDto>();

        foreach (var type in types)
        {
            var journals = (await _journals.GetAllAsync(
                tenantId, journalType: type, status: "Posted",
                fromDate: fromDate, toDate: toDate, pageNumber: 1, pageSize: 50_000)).ToList();
            if (journals.Count == 0) continue;

            var amount = Math.Round(journals.Sum(j => Math.Max(j.TotalDebit, j.TotalCredit)), 2);
            var line = new CashFlowLineDto
            {
                JournalType = type,
                Direction = IsReceiptVoucher(type) ? "Inflow" : "Outflow",
                Count = journals.Count,
                Amount = amount
            };
            if (IsReceiptVoucher(type)) inflows.Add(line);
            else outflows.Add(line);
        }

        var totalIn = Math.Round(inflows.Sum(l => l.Amount), 2);
        var totalOut = Math.Round(outflows.Sum(l => l.Amount), 2);
        var cashLedger = await GetCashBankLedgerSummaryAsync(tenantId, fromDate, toDate);
        var opening = Math.Round(cashLedger.Lines.Sum(l => l.OpeningBalance), 2);
        var closing = Math.Round(cashLedger.Lines.Sum(l => l.ClosingBalance), 2);
        return new CashFlowReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            CurrencyCode = currencyCode ?? "USD",
            OpeningCash = opening,
            Inflows = inflows,
            Outflows = outflows,
            TotalInflows = totalIn,
            TotalOutflows = totalOut,
            NetChange = Math.Round(totalIn - totalOut, 2),
            ClosingCash = closing
        };
    }

    public async Task<ChequeRegisterReportDto> GetChequeRegisterAsync(
        Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null,
        string? chequeStatus = null, string? journalType = null, string? search = null)
    {
        if (tenantId == Guid.Empty)
            return new ChequeRegisterReportDto { FromDate = fromDate, ToDate = toDate, ChequeStatus = chequeStatus, JournalType = journalType, Lines = [], TotalCount = 0 };

        List<Journal> journals;
        if (_db != null)
        {
            var q = _db.Journals.AsNoTracking()
                .Where(j => j.TenantId == tenantId
                            && j.ChequeNumber != null && j.ChequeNumber != "");
            if (fromDate.HasValue) q = q.Where(j => j.JournalDate >= fromDate.Value.Date);
            if (toDate.HasValue) q = q.Where(j => j.JournalDate <= toDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(journalType)) q = q.Where(j => j.JournalType == journalType);
            if (!string.IsNullOrWhiteSpace(chequeStatus))
                q = q.Where(j => j.ChequeStatus == chequeStatus);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(j =>
                    (j.ChequeNumber != null && j.ChequeNumber.Contains(term))
                    || (j.JournalNumber != null && j.JournalNumber.Contains(term))
                    || (j.ReferenceNo != null && j.ReferenceNo.Contains(term)));
            }
            journals = await q.OrderByDescending(j => j.JournalDate).ThenBy(j => j.ChequeNumber).ToListAsync();
        }
        else
        {
            journals = (await _journals.GetAllAsync(
                tenantId, journalType: journalType, fromDate: fromDate, toDate: toDate,
                pageNumber: 1, pageSize: 50_000))
                .Where(j => !string.IsNullOrWhiteSpace(j.ChequeNumber))
                .Where(j => string.IsNullOrWhiteSpace(chequeStatus)
                            || string.Equals(j.ChequeStatus, chequeStatus, StringComparison.OrdinalIgnoreCase))
                .Where(j => string.IsNullOrWhiteSpace(search)
                            || (j.ChequeNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                            || (j.JournalNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .OrderByDescending(j => j.JournalDate)
                .ToList();
        }

        var accounts = (await _accounts.GetAllAsync(tenantId)).ToList();
        var lines = journals.Select(j =>
        {
            var bank = j.CashOrBankAccountId.HasValue
                ? accounts.FirstOrDefault(a => a.AccountId == j.CashOrBankAccountId)
                : null;
            var status = string.IsNullOrWhiteSpace(j.ChequeStatus) ? "Outstanding" : j.ChequeStatus!;
            var isReceipt = IsReceiptVoucher(j.JournalType);
            return new ChequeRegisterLineDto
            {
                JournalId = j.JournalId,
                JournalNumber = j.JournalNumber,
                JournalType = j.JournalType,
                JournalDate = j.JournalDate,
                ChequeDate = j.ReferenceDate ?? j.JournalDate,
                ChequeNumber = j.ChequeNumber!,
                ChequeStatus = status,
                ChequeStatusAt = j.ChequeStatusAt,
                ChequeStatusNotes = j.ChequeStatusNotes,
                VoucherStatus = j.Status,
                Amount = Math.Round(Math.Max(j.TotalDebit, j.TotalCredit), 2),
                Direction = isReceipt ? "Inflow" : "Outflow",
                BankAccountId = j.CashOrBankAccountId,
                BankAccountName = bank?.AccountName,
                PaidBy = j.PaidBy,
                ReceivedBy = j.ReceivedBy,
                ReferenceNo = j.ReferenceNo,
                Description = j.Description
            };
        }).ToList();

        return new ChequeRegisterReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            ChequeStatus = chequeStatus,
            JournalType = journalType,
            Lines = lines,
            TotalCount = lines.Count,
            TotalAmount = Math.Round(lines.Sum(l => l.Amount), 2),
            OutstandingAmount = Math.Round(lines.Where(l => l.ChequeStatus == "Outstanding").Sum(l => l.Amount), 2),
            ClearedAmount = Math.Round(lines.Where(l => l.ChequeStatus == "Cleared").Sum(l => l.Amount), 2),
            BouncedAmount = Math.Round(lines.Where(l => l.ChequeStatus == "Bounced").Sum(l => l.Amount), 2)
        };
    }

    public async Task<bool> SetChequeStatusAsync(Guid journalId, SetChequeStatusDto dto, Guid tenantId, Guid userId)
    {
        var allowed = new[] { "Outstanding", "Cleared", "Bounced", "Cancelled" };
        var status = (dto.Status ?? string.Empty).Trim();
        if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase)) return false;

        var journal = await _journals.GetByIdAsync(journalId, tenantId);
        if (journal == null || string.IsNullOrWhiteSpace(journal.ChequeNumber)) return false;

        journal.ChequeStatus = allowed.First(a => a.Equals(status, StringComparison.OrdinalIgnoreCase));
        journal.ChequeStatusAt = DateTime.UtcNow;
        if (dto.Notes != null) journal.ChequeStatusNotes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        journal.UpdatedAt = DateTime.UtcNow;
        journal.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _journals.UpdateAsync(journal);
        return true;
    }

    public async Task<BudgetVsActualReportDto> GetBudgetVsActualAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, Guid? budgetId = null)
    {
        if (tenantId == Guid.Empty || _db == null)
            return new BudgetVsActualReportDto { FromDate = fromDate, ToDate = toDate, BudgetId = budgetId };

        var q = _db.Budgets.AsNoTracking()
            .Where(b => b.TenantId == tenantId);
        if (budgetId.HasValue && budgetId != Guid.Empty)
            q = q.Where(b => b.BudgetId == budgetId || b.Id == budgetId);

        q = q.Where(b =>
            (b.BudgetPeriodStart != default ? b.BudgetPeriodStart : b.StartDate) <= toDate
            && (b.BudgetPeriodEnd != default ? b.BudgetPeriodEnd : b.EndDate) >= fromDate);

        var budgets = await q.OrderBy(b => b.BudgetName).ToListAsync();
        var tb = await GetTrialBalanceAsync(tenantId, fromDate, toDate);
        var accounts = (await _accounts.GetAllAsync(tenantId)).ToList();
        var tbByAccount = tb.Lines.ToDictionary(l => l.AccountId, l => l);

        var lines = new List<BudgetVsActualLineDto>();
        foreach (var b in budgets)
        {
            var account = accounts.FirstOrDefault(a => a.AccountId == b.AccountId);
            tbByAccount.TryGetValue(b.AccountId, out var tbLine);
            var actual = ComputeBudgetActual(account?.AccountType ?? tbLine?.AccountType, tbLine);
            var budgetAmt = Math.Round(b.BudgetAmount, 2);
            var variance = Math.Round(actual - budgetAmt, 2);
            var pct = budgetAmt == 0 ? (actual == 0 ? 0 : 100m) : Math.Round(variance / budgetAmt * 100m, 2);
            lines.Add(new BudgetVsActualLineDto
            {
                BudgetId = b.BudgetId != Guid.Empty ? b.BudgetId : b.Id,
                BudgetName = b.BudgetName,
                AccountId = b.AccountId,
                AccountCode = account?.AccountCode ?? tbLine?.AccountCode,
                AccountName = account?.AccountName ?? tbLine?.AccountName,
                AccountType = account?.AccountType ?? tbLine?.AccountType,
                PeriodStart = b.BudgetPeriodStart != default ? b.BudgetPeriodStart : b.StartDate,
                PeriodEnd = b.BudgetPeriodEnd != default ? b.BudgetPeriodEnd : b.EndDate,
                BudgetAmount = budgetAmt,
                ActualAmount = actual,
                Variance = variance,
                VariancePercent = pct,
                Status = b.Status
            });
        }

        return new BudgetVsActualReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BudgetId = budgetId,
            Lines = lines,
            TotalBudget = Math.Round(lines.Sum(l => l.BudgetAmount), 2),
            TotalActual = Math.Round(lines.Sum(l => l.ActualAmount), 2),
            TotalVariance = Math.Round(lines.Sum(l => l.Variance), 2)
        };
    }

    private static decimal ComputeBudgetActual(string? accountType, TrialBalanceLineDto? tbLine)
    {
        if (tbLine == null) return 0m;
        var type = accountType?.Trim() ?? string.Empty;
        if (type.Equals("Revenue", StringComparison.OrdinalIgnoreCase)
            || type.Equals("Income", StringComparison.OrdinalIgnoreCase))
            return Math.Round(tbLine.Credit - tbLine.Debit, 2);
        if (type.Equals("Expense", StringComparison.OrdinalIgnoreCase)
            || type.Equals("CostOfGoodsSold", StringComparison.OrdinalIgnoreCase)
            || type.Equals("COGS", StringComparison.OrdinalIgnoreCase))
            return Math.Round(tbLine.Debit - tbLine.Credit, 2);
        return Math.Round(tbLine.Balance, 2);
    }

    private async Task<List<(Journal Journal, JournalEntry Entry)>> LoadPostedEntriesAsync(
        Guid tenantId, DateTime? fromDate, DateTime? toDate)
    {
        var journals = (await _journals.GetAllAsync(
            tenantId, status: "Posted", fromDate: fromDate, toDate: toDate, pageNumber: 1, pageSize: 50_000)).ToList();

        var result = new List<(Journal, JournalEntry)>();
        foreach (var j in journals)
        {
            IEnumerable<JournalEntry> entries = j.JournalEntries is { Count: > 0 }
                ? j.JournalEntries
                : await _journals.GetEntriesByJournalIdAsync(j.JournalId, tenantId);
            foreach (var e in entries)
                result.Add((j, e));
        }
        return result;
    }

    private List<TrialBalanceLineDto> BuildTbLines(IEnumerable<JournalEntry> entries, List<ChartOfAccount> accounts)
    {
        return entries
            .GroupBy(e => e.AccountId)
            .Select(g =>
            {
                var account = accounts.FirstOrDefault(a => a.AccountId == g.Key);
                var debit = Math.Round(g.Sum(x => x.Debit), 2);
                var credit = Math.Round(g.Sum(x => x.Credit), 2);
                return new TrialBalanceLineDto
                {
                    AccountId = g.Key,
                    AccountCode = account?.AccountCode ?? g.Key.ToString("N")[..8],
                    AccountName = account?.AccountName ?? "(unknown)",
                    AccountType = account?.AccountType ?? string.Empty,
                    Level = account == null ? 0 : ComputeLevelFromList(account, accounts),
                    Debit = debit,
                    Credit = credit,
                    Balance = Math.Round(debit - credit, 2)
                };
            })
            .Where(l => l.Debit != 0 || l.Credit != 0)
            .ToList();
    }

    private static TrialBalanceLineDto CloneWithBalance(TrialBalanceLineDto line, decimal balance) => new()
    {
        AccountId = line.AccountId,
        AccountCode = line.AccountCode,
        AccountName = line.AccountName,
        AccountType = line.AccountType,
        Level = line.Level,
        Debit = line.Debit,
        Credit = line.Credit,
        Balance = balance
    };

    private static bool IsRevenueType(string type) =>
        type.Equals("Revenue", StringComparison.OrdinalIgnoreCase)
        || type.Equals("Income", StringComparison.OrdinalIgnoreCase);

    private static bool IsExpenseType(string type) =>
        type.Equals("Expense", StringComparison.OrdinalIgnoreCase)
        || type.Equals("CostOfGoodsSold", StringComparison.OrdinalIgnoreCase);

    private static bool IsAssetType(string type) =>
        type.Equals("Asset", StringComparison.OrdinalIgnoreCase);

    private static bool IsLiabilityType(string type) =>
        type.Equals("Liability", StringComparison.OrdinalIgnoreCase);

    private static bool IsEquityType(string type) =>
        type.Equals("Equity", StringComparison.OrdinalIgnoreCase);

    private async Task<int> ComputeLevelAsync(ChartOfAccount account, Guid tenantId)
    {
        var level = 1;
        var current = account;
        var guard = 0;
        while (current.ParentAccountId.HasValue && guard++ < 20)
        {
            var parent = await _accounts.GetByIdAsync(current.ParentAccountId.Value, tenantId);
            if (parent == null) break;
            level++;
            current = parent;
        }
        return level;
    }

    private static int ComputeLevelFromList(ChartOfAccount account, IReadOnlyList<ChartOfAccount> all)
    {
        var level = 1;
        var current = account;
        var guard = 0;
        while (current.ParentAccountId.HasValue && guard++ < 20)
        {
            var parent = all.FirstOrDefault(a => a.AccountId == current.ParentAccountId);
            if (parent == null) break;
            level++;
            current = parent;
        }
        return level;
    }

    private async Task RememberVoucherDefaultsAsync(Guid? cashOrBankAccountId, List<CreateVoucherLineDto> offsetLines)
    {
        if (_settings == null) return;

        if (cashOrBankAccountId is Guid cashId && cashId != Guid.Empty)
            await _settings.SetValueAsync(SettingsCatalog.LastCashOrBankAccountIdKey, cashId.ToString("D"));

        var costCenterId = offsetLines
            .Select(l => l.CostCenterId)
            .FirstOrDefault(id => id is Guid cc && cc != Guid.Empty);
        if (costCenterId is Guid ccId && ccId != Guid.Empty)
            await _settings.SetValueAsync(SettingsCatalog.LastCostCenterIdKey, ccId.ToString("D"));
    }

    private static bool IsPaymentVoucher(string journalType) =>
        string.Equals(journalType, "CashPayment", StringComparison.OrdinalIgnoreCase)
        || string.Equals(journalType, "BankPayment", StringComparison.OrdinalIgnoreCase);

    private static bool IsReceiptVoucher(string journalType) =>
        string.Equals(journalType, "CashReceipt", StringComparison.OrdinalIgnoreCase)
        || string.Equals(journalType, "BankReceipt", StringComparison.OrdinalIgnoreCase);

    private static List<CreateVoucherLineDto> ResolveOffsetLines(JournalCreateDto dto)
    {
        if (dto.OffsetLines is { Count: > 0 }) return dto.OffsetLines;
        if (dto.DebitLines is { Count: > 0 }) return dto.DebitLines;
        if (dto.CreditLines is { Count: > 0 }) return dto.CreditLines;
        return [];
    }

    private static string BuildJournalNumber(string journalType, Guid id)
    {
        var prefix = journalType switch
        {
            "CashPayment" => "CPV",
            "CashReceipt" => "CRV",
            "BankPayment" => "BPV",
            "BankReceipt" => "BRV",
            "Journal" => "JV",
            _ => "JV"
        };
        return $"{prefix}-{DateTime.UtcNow:yyMM}-{id.ToString("N")[..6]}".ToUpperInvariant();
    }

    private static JournalDto MapJournal(Journal entity, IEnumerable<JournalEntry>? entries) => new()
    {
        Id = entity.Id,
        JournalId = entity.JournalId,
        TenantId = entity.TenantId,
        JournalType = entity.JournalType,
        JournalNumber = entity.JournalNumber,
        ReferenceNo = entity.ReferenceNo,
        JournalDate = entity.JournalDate,
        ReferenceDate = entity.ReferenceDate,
        Description = entity.Description,
        CurrencyCode = entity.CurrencyCode,
        ExchangeRate = entity.ExchangeRate,
        TotalDebit = entity.TotalDebit,
        TotalCredit = entity.TotalCredit,
        Status = entity.Status,
        ChequeNumber = entity.ChequeNumber,
        ChequeStatus = entity.ChequeStatus,
        ChequeStatusAt = entity.ChequeStatusAt,
        ChequeStatusNotes = entity.ChequeStatusNotes,
        PaidBy = entity.PaidBy,
        ReceivedBy = entity.ReceivedBy,
        CashOrBankAccountId = entity.CashOrBankAccountId,
        InvoiceNumber = entity.InvoiceNumber,
        EntityReferenceTable = entity.EntityReferenceTable,
        EntityReferenceId = entity.EntityReferenceId,
        CreatedAt = entity.CreatedAt,
        CreatedBy = Guid.TryParse(entity.CreatedBy, out var cb) ? cb : null,
        PostedAt = entity.PostedAt,
        PostedBy = entity.PostedBy,
        ApprovedAt = entity.ApprovedAt,
        ApprovedBy = entity.ApprovedBy,
        IsLocked = entity.IsLocked,
        FinancialYearId = entity.FinancialYearId,
        JournalEntries = entries?.Select(MapEntry).ToList()
    };

    private static JournalEntryDto MapEntry(JournalEntry e) => new()
    {
        Id = e.Id,
        EntryId = e.EntryId,
        TenantId = e.TenantId,
        JournalId = e.JournalId,
        AccountId = e.AccountId,
        CostCenterId = e.CostCenterId,
        ProjectId = e.ProjectId,
        Debit = e.Debit,
        Credit = e.Credit,
        Narration = e.Narration,
        EntityReferenceTable = e.EntityReferenceTable,
        EntityReferenceId = e.EntityReferenceId,
        CreatedAt = e.CreatedAt
    };

    private static ChartOfAccountDto MapAccount(ChartOfAccount entity, IReadOnlyList<ChartOfAccount> all)
    {
        var hasChildren = all.Any(a => a.ParentAccountId == entity.AccountId);
        return new ChartOfAccountDto
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            TenantId = entity.TenantId,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountType = entity.AccountType,
            ParentAccountId = entity.ParentAccountId,
            ParentAccountName = entity.ParentAccountId.HasValue
                ? all.FirstOrDefault(a => a.AccountId == entity.ParentAccountId)?.AccountName
                : null,
            CurrencyCode = entity.CurrencyCode,
            Remarks = entity.Remarks,
            Level = ComputeLevelFromList(entity, all),
            HasChildren = hasChildren,
            IsActive = entity.IsActive,
            IsPostingAllowed = entity.IsPostingAllowed,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = Guid.TryParse(entity.CreatedBy, out var cb) ? cb : null,
            UpdatedBy = Guid.TryParse(entity.UpdatedBy, out var ub) ? ub : null
        };
    }
}
