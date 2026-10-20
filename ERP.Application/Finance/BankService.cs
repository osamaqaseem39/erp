using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;
using IBankAccountRepository = ERP.Infrastructure.Repositories.Finance.IBankAccountRepository;
using IBankRepository = ERP.Infrastructure.Repositories.Finance.IBankRepository;

namespace ERP.Application.Finance;

public class BankService : IBankService
{
    private readonly IBankRepository _banks;
    private readonly IBankAccountRepository _accounts;
    private readonly ERPDbContext _db;

    public BankService(IBankRepository banks, IBankAccountRepository accounts, ERPDbContext db)
    {
        _banks = banks;
        _accounts = accounts;
        _db = db;
    }

    public async Task<BankDto?> GetByIdAsync(Guid id)
    {
        var entity = await _banks.GetByIdAsync(id);
        return entity == null ? null : MapBank(entity);
    }

    public async Task<BankDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _banks.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapBank(entity);
    }

    public async Task<IEnumerable<BankDto>> GetAllAsync()
        => (await _banks.GetAllAsync()).Select(MapBank);

    public async Task<IEnumerable<BankDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BankDto>();
        return (await _banks.GetAllAsync(tenantId)).Select(MapBank);
    }

    public async Task<Guid> CreateAsync(CreateBankDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BankName) || dto.TenantId == Guid.Empty) return Guid.Empty;
        var entity = new Bank
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            BankName = dto.BankName.Trim(),
            BankCode = string.IsNullOrWhiteSpace(dto.BankCode)
                ? dto.BankName.Trim().ToUpperInvariant()[..Math.Min(8, dto.BankName.Trim().Length)]
                : dto.BankCode.Trim(),
            SwiftCode = NullIfEmpty(dto.SwiftCode),
            Address = NullIfEmpty(dto.Address),
            City = NullIfEmpty(dto.City),
            State = NullIfEmpty(dto.State),
            Country = NullIfEmpty(dto.Country),
            PostalCode = NullIfEmpty(dto.PostalCode),
            Phone = NullIfEmpty(dto.Phone),
            Email = NullIfEmpty(dto.Email),
            Website = NullIfEmpty(dto.Website),
            IsActive = dto.IsActive,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        return await _banks.CreateAsync(entity);
    }

    public async Task<Guid> CreateAsync(CreateBankDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        dto.TenantId = tenantId;
        dto.CreatedBy = userId.ToString();
        return await CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateBankDto dto)
    {
        var entity = await _banks.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        if (!string.IsNullOrWhiteSpace(dto.BankName)) entity.BankName = dto.BankName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.BankCode)) entity.BankCode = dto.BankCode.Trim();
        entity.SwiftCode = dto.SwiftCode;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.State = dto.State;
        entity.Country = dto.Country;
        entity.PostalCode = dto.PostalCode;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Website = dto.Website;
        entity.IsActive = dto.IsActive;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _banks.UpdateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateBankDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _banks.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        dto.UpdatedBy = userId.ToString();
        return await UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var accounts = await _accounts.GetByBankAsync(id);
        if (accounts.Any()) return false;
        return await _banks.DeleteAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _banks.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        return await DeleteAsync(id);
    }

    public async Task<IEnumerable<BankDto>> GetActiveAsync()
        => (await _banks.GetActiveAsync()).Select(MapBank);

    public async Task<IEnumerable<BankDto>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BankDto>();
        return (await _banks.GetActiveAsync(tenantId)).Select(MapBank);
    }

    public async Task<IEnumerable<BankDto>> SearchAsync(string searchTerm)
        => await SearchAsync(searchTerm, Guid.Empty);

    public async Task<IEnumerable<BankDto>> SearchAsync(string searchTerm, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BankDto>();
        var term = searchTerm?.Trim() ?? string.Empty;
        var all = await _banks.GetAllAsync(tenantId);
        if (string.IsNullOrEmpty(term)) return all.Select(MapBank);
        return all.Where(b =>
                b.BankName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || b.BankCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (b.SwiftCode?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(MapBank);
    }

    public async Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(Guid bankId)
        => (await _accounts.GetByBankAsync(bankId)).Select(MapAccount);

    public async Task<BankAccountDto?> GetBankAccountByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _accounts.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return null;
        return MapAccount(entity);
    }

    public async Task<IEnumerable<BankAccountDto>> GetAllBankAccountsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BankAccountDto>();
        return (await _accounts.GetAllAsync())
            .Where(a => a.TenantId == tenantId)
            .Select(MapAccount);
    }

    public async Task<Guid> CreateBankAccountAsync(CreateBankAccountDto dto)
        => await AddBankAccountAsync(dto, dto.TenantId, Guid.TryParse(dto.CreatedBy, out var u) ? u : Guid.Empty);

    public async Task<bool> UpdateBankAccountAsync(UpdateBankAccountDto dto)
    {
        var entity = await _accounts.GetByIdAsync(dto.Id);
        if (entity == null) return false;
        ApplyAccountUpdate(dto, entity);
        return await _accounts.UpdateAsync(entity);
    }

    public async Task<bool> DeleteBankAccountAsync(Guid id)
        => await DeleteBankAccountAsync(id, Guid.Empty);

    public async Task<Guid> AddBankAccountAsync(CreateBankAccountDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.BankId == Guid.Empty || string.IsNullOrWhiteSpace(dto.AccountNumber))
            return Guid.Empty;
        var bank = await _banks.GetByIdAsync(dto.BankId, tenantId);
        if (bank == null) return Guid.Empty;

        var existing = await _accounts.GetByAccountNumberAsync(dto.AccountNumber);
        if (existing != null && existing.TenantId == tenantId) return Guid.Empty;

        var entity = new BankAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId != Guid.Empty ? dto.CompanyId : bank.CompanyId,
            BankId = dto.BankId,
            AccountName = string.IsNullOrWhiteSpace(dto.AccountName) ? dto.AccountNumber : dto.AccountName.Trim(),
            AccountNumber = dto.AccountNumber.Trim(),
            AccountType = string.IsNullOrWhiteSpace(dto.AccountType) ? "Checking" : dto.AccountType,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
            Balance = dto.Balance,
            IBAN = dto.IBAN,
            SWIFT = dto.SWIFT,
            GLAccountId = dto.GLAccountId,
            IsActive = dto.IsActive,
            CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        return await _accounts.CreateAsync(entity);
    }

    public async Task<Guid> UpdateBankAccountAsync(UpdateBankAccountDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = await _accounts.GetByIdAsync(dto.Id);
        if (entity == null || entity.TenantId != tenantId) return Guid.Empty;
        ApplyAccountUpdate(dto, entity);
        entity.UpdatedBy = userId.ToString();
        return await _accounts.UpdateAsync(entity) ? entity.Id : Guid.Empty;
    }

    public async Task<bool> DeleteBankAccountAsync(Guid bankAccountId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _accounts.GetByIdAsync(bankAccountId);
        if (entity == null || entity.TenantId != tenantId) return false;

        var hasTx = await _db.BankTransactions.AnyAsync(t => t.BankAccountId == bankAccountId);
        if (hasTx) return false;
        return await _accounts.DeleteAsync(bankAccountId);
    }

    public async Task<Guid> AddBankTransactionAsync(CreateBankTransactionDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.BankAccountId == Guid.Empty || dto.Amount == 0) return Guid.Empty;
        var account = await _accounts.GetByIdAsync(dto.BankAccountId);
        if (account == null || account.TenantId != tenantId) return Guid.Empty;

        var txId = Guid.NewGuid();
        var tx = new BankTransaction
        {
            BankTxId = txId,
            BankAccountId = dto.BankAccountId,
            TenantId = tenantId,
            BankReference = dto.BankReference,
            TransactionDate = dto.TransactionDate == default ? DateTime.UtcNow.Date : dto.TransactionDate,
            Amount = dto.Amount,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? account.Currency : dto.CurrencyCode,
            Description = dto.Description,
            ImportedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId
        };
        _db.BankTransactions.Add(tx);
        account.Balance = Math.Round(account.Balance + dto.Amount, 2);
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return txId;
    }

    public async Task<bool> ImportBankTransactionsAsync(
        Guid bankAccountId,
        List<CreateBankTransactionDto> transactions,
        Guid tenantId,
        Guid userId)
    {
        if (transactions == null || transactions.Count == 0) return false;
        foreach (var row in transactions)
        {
            row.BankAccountId = bankAccountId;
            var id = await AddBankTransactionAsync(row, tenantId, userId);
            if (id == Guid.Empty) return false;
        }
        return true;
    }

    public async Task<IEnumerable<BankTransactionDto>> GetBankTransactionsAsync(
        Guid bankAccountId, Guid tenantId, bool? unreconciledOnly = null, DateTime? toDate = null) =>
        await ListBankTransactionsAsync(tenantId, bankAccountId, unreconciledOnly, toDate);

    public async Task<IEnumerable<BankTransactionDto>> ListBankTransactionsAsync(
        Guid tenantId, Guid? bankAccountId = null, bool? unreconciledOnly = null, DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BankTransactionDto>();
        var q = _db.BankTransactions.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (bankAccountId.HasValue && bankAccountId != Guid.Empty)
            q = q.Where(t => t.BankAccountId == bankAccountId);
        if (unreconciledOnly == true) q = q.Where(t => !t.IsReconciled);
        if (toDate.HasValue) q = q.Where(t => t.TransactionDate <= toDate.Value);
        var rows = await q.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).ToListAsync();
        return rows.Select(MapTransaction);
    }

    public async Task<bool> MatchBankTransactionToPaymentAsync(BankTransactionMatchDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var tx = await _db.BankTransactions.FirstOrDefaultAsync(t =>
            t.BankTxId == dto.BankTransactionId && t.TenantId == tenantId);
        if (tx == null || tx.MatchedPaymentId.HasValue) return false;
        if (dto.PaymentId == Guid.Empty) return false;

        var payment = await _db.Payments.FirstOrDefaultAsync(p =>
            p.Id == dto.PaymentId && p.TenantId == tenantId);
        if (payment == null) return false;

        var expected = Math.Abs(tx.Amount);
        var matchAmount = dto.MatchAmount > 0 ? dto.MatchAmount : Math.Abs(payment.Amount);
        if (Math.Abs(matchAmount - expected) > 0.01m) return false;
        if (Math.Abs(Math.Abs(payment.Amount) - expected) > 0.01m) return false;

        tx.MatchedPaymentId = dto.PaymentId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnmatchBankTransactionAsync(Guid bankTxId, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var tx = await _db.BankTransactions.FirstOrDefaultAsync(t =>
            t.BankTxId == bankTxId && t.TenantId == tenantId);
        if (tx == null || !tx.MatchedPaymentId.HasValue) return false;
        if (tx.IsReconciled && tx.ReconciliationId.HasValue)
        {
            var recon = await _db.BankReconciliations.FirstOrDefaultAsync(r => r.ReconciliationId == tx.ReconciliationId);
            if (recon != null && string.Equals(recon.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                return false;
        }
        tx.MatchedPaymentId = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Guid> CreateReconciliationAsync(CreateBankReconciliationDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.BankAccountId == Guid.Empty) return Guid.Empty;
        var account = await _accounts.GetByIdAsync(dto.BankAccountId);
        if (account == null || account.TenantId != tenantId) return Guid.Empty;

        var openExists = await _db.BankReconciliations.AnyAsync(r =>
            r.BankAccountId == dto.BankAccountId
            && r.TenantId == tenantId
            && r.Status == "Open");
        if (openExists) return Guid.Empty;

        var asOf = dto.ReconciliationDate == default ? DateTime.UtcNow.Date : dto.ReconciliationDate.Date;
        var opening = dto.OpeningBalance;
        var lastClosed = await _db.BankReconciliations
            .Where(r => r.BankAccountId == dto.BankAccountId
                        && r.TenantId == tenantId
                        && r.Status == "Closed")
            .OrderByDescending(r => r.ReconciliationDate)
            .ThenByDescending(r => r.ClosedAt)
            .FirstOrDefaultAsync();
        if (lastClosed != null && dto.OpeningBalance == 0)
            opening = lastClosed.ClosingBalance;

        var id = Guid.NewGuid();
        var recon = new BankReconciliation
        {
            ReconciliationId = id,
            BankAccountId = dto.BankAccountId,
            TenantId = tenantId,
            ReconciliationDate = asOf,
            OpeningBalance = opening,
            ClosingBalance = dto.ClosingBalance,
            Difference = Math.Round(dto.ClosingBalance - opening, 2),
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId
        };
        _db.BankReconciliations.Add(recon);
        await _db.SaveChangesAsync();
        return id;
    }

    public async Task<bool> SetTransactionReconciledAsync(
        Guid reconciliationId, Guid bankTxId, bool reconciled, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var recon = await _db.BankReconciliations.FirstOrDefaultAsync(r =>
            r.ReconciliationId == reconciliationId && r.TenantId == tenantId);
        if (recon == null || !string.Equals(recon.Status, "Open", StringComparison.OrdinalIgnoreCase))
            return false;

        var tx = await _db.BankTransactions.FirstOrDefaultAsync(t =>
            t.BankTxId == bankTxId
            && t.BankAccountId == recon.BankAccountId
            && t.TenantId == tenantId);
        if (tx == null) return false;
        if (tx.TransactionDate > recon.ReconciliationDate) return false;

        if (reconciled)
        {
            if (tx.IsReconciled && tx.ReconciliationId.HasValue && tx.ReconciliationId != reconciliationId)
                return false;
            tx.IsReconciled = true;
            tx.ReconciliationId = reconciliationId;
            var existingLine = await _db.BankReconciliationLines.FirstOrDefaultAsync(l =>
                l.ReconciliationId == reconciliationId && l.BankTransactionId == bankTxId && l.TenantId == tenantId);
            if (existingLine == null)
            {
                _db.BankReconciliationLines.Add(new BankReconciliationLine
                {
                    TenantId = tenantId,
                    ReconciliationId = reconciliationId,
                    BankTransactionId = bankTxId,
                    MatchedAmount = Math.Abs(tx.Amount),
                    IsMatched = true,
                    CreatedBy = userId.ToString()
                });
            }
            else
            {
                existingLine.IsMatched = true;
                existingLine.MatchedAmount = Math.Abs(tx.Amount);
                existingLine.UpdatedAt = DateTime.UtcNow;
                existingLine.UpdatedBy = userId.ToString();
            }
        }
        else
        {
            if (!tx.IsReconciled || tx.ReconciliationId != reconciliationId) return false;
            tx.IsReconciled = false;
            tx.ReconciliationId = null;
            var line = await _db.BankReconciliationLines.FirstOrDefaultAsync(l =>
                l.ReconciliationId == reconciliationId && l.BankTransactionId == bankTxId && l.TenantId == tenantId);
            if (line != null)
            {
                line.IsMatched = false;
                line.UpdatedAt = DateTime.UtcNow;
                line.UpdatedBy = userId.ToString();
            }
        }

        await RecalcDifferenceAsync(recon, tenantId);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseReconciliationAsync(Guid reconciliationId, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var recon = await _db.BankReconciliations.FirstOrDefaultAsync(r =>
            r.ReconciliationId == reconciliationId && r.TenantId == tenantId);
        if (recon == null || !string.Equals(recon.Status, "Open", StringComparison.OrdinalIgnoreCase))
            return false;

        await RecalcDifferenceAsync(recon, tenantId);
        if (Math.Abs(recon.Difference) > 0.01m) return false;

        var lineCount = await _db.BankReconciliationLines.CountAsync(l =>
            l.ReconciliationId == reconciliationId && l.TenantId == tenantId && l.IsMatched);
        var clearedCount = await _db.BankTransactions.CountAsync(t =>
            t.ReconciliationId == reconciliationId && t.TenantId == tenantId && t.IsReconciled);
        // When recon lines are in use, every cleared tx must have a matched line
        if (lineCount > 0 && lineCount < clearedCount) return false;

        recon.Status = "Closed";
        recon.ClosedAt = DateTime.UtcNow;
        recon.ClosedBy = userId == Guid.Empty ? null : userId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<BankReconciliationDto?> GetReconciliationAsync(Guid reconciliationId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var recon = await _db.BankReconciliations
            .FirstOrDefaultAsync(r => r.ReconciliationId == reconciliationId && r.TenantId == tenantId);
        if (recon == null) return null;
        await RecalcDifferenceAsync(recon, tenantId);
        if (string.Equals(recon.Status, "Open", StringComparison.OrdinalIgnoreCase))
            await _db.SaveChangesAsync();
        var account = await _accounts.GetByIdAsync(recon.BankAccountId);
        var clearedCount = await _db.BankTransactions.CountAsync(t =>
            t.ReconciliationId == recon.ReconciliationId && t.IsReconciled);
        var cleared = await ClearedNetAsync(recon.ReconciliationId, tenantId);
        return MapReconciliation(recon, account?.AccountName, cleared, clearedCount);
    }

    public async Task<IEnumerable<BankReconciliationDto>> ListReconciliationsAsync(
        Guid bankAccountId, Guid tenantId, string? status = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BankReconciliationDto>();
        var q = _db.BankReconciliations.AsNoTracking()
            .Where(r => r.BankAccountId == bankAccountId && r.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);
        var rows = await q.OrderByDescending(r => r.ReconciliationDate).ToListAsync();
        var account = await _accounts.GetByIdAsync(bankAccountId);
        var result = new List<BankReconciliationDto>();
        foreach (var r in rows)
        {
            var cleared = await ClearedNetAsync(r.ReconciliationId, tenantId);
            var count = await _db.BankTransactions.CountAsync(t =>
                t.ReconciliationId == r.ReconciliationId && t.IsReconciled);
            result.Add(MapReconciliation(r, account?.AccountName, cleared, count));
        }
        return result;
    }

    private async Task RecalcDifferenceAsync(BankReconciliation recon, Guid tenantId)
    {
        var cleared = await ClearedNetAsync(recon.ReconciliationId, tenantId);
        recon.Difference = Math.Round(recon.ClosingBalance - (recon.OpeningBalance + cleared), 2);
    }

    private async Task<decimal> ClearedNetAsync(Guid reconciliationId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return 0m;
        var q = _db.BankTransactions.Where(t =>
            t.ReconciliationId == reconciliationId && t.IsReconciled && t.TenantId == tenantId);
        return Math.Round(await q.SumAsync(t => (decimal?)t.Amount) ?? 0m, 2);
    }

    private static BankReconciliationDto MapReconciliation(
        BankReconciliation r, string? accountName, decimal clearedBalance, int clearedCount) => new()
    {
        Id = r.ReconciliationId,
        ReconciliationId = r.ReconciliationId,
        BankAccountId = r.BankAccountId,
        ReconciliationDate = r.ReconciliationDate,
        OpeningBalance = r.OpeningBalance,
        ClosingBalance = r.ClosingBalance,
        ClearedBalance = Math.Round(r.OpeningBalance + clearedBalance, 2),
        Difference = r.Difference,
        Status = r.Status,
        ClosedAt = r.ClosedAt,
        ClosedBy = r.ClosedBy,
        BankAccountName = accountName,
        ReconciledTransactionCount = clearedCount,
        CreatedAt = r.CreatedAt,
        CreatedBy = r.CreatedBy?.ToString()
    };

    private static BankTransactionDto MapTransaction(BankTransaction t) => new()
    {
        Id = t.BankTxId,
        BankAccountId = t.BankAccountId,
        BankReference = t.BankReference,
        TransactionDate = t.TransactionDate,
        Amount = t.Amount,
        CurrencyCode = t.CurrencyCode,
        Description = t.Description,
        ImportedAt = t.ImportedAt,
        MatchedPaymentId = t.MatchedPaymentId,
        ReconciliationId = t.ReconciliationId,
        IsReconciled = t.IsReconciled,
        CreatedAt = t.CreatedAt,
        CreatedBy = t.CreatedBy?.ToString()
    };

    private static void ApplyAccountUpdate(UpdateBankAccountDto dto, BankAccount entity)
    {
        if (dto.BankId != Guid.Empty) entity.BankId = dto.BankId;
        if (!string.IsNullOrWhiteSpace(dto.AccountName)) entity.AccountName = dto.AccountName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.AccountNumber)) entity.AccountNumber = dto.AccountNumber.Trim();
        if (!string.IsNullOrWhiteSpace(dto.AccountType)) entity.AccountType = dto.AccountType;
        if (!string.IsNullOrWhiteSpace(dto.Currency)) entity.Currency = dto.Currency;
        entity.Balance = dto.Balance;
        entity.IBAN = dto.IBAN;
        entity.SWIFT = dto.SWIFT;
        if (dto.GLAccountId != Guid.Empty) entity.GLAccountId = dto.GLAccountId;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    private static BankDto MapBank(Bank entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        BankName = entity.BankName,
        BankCode = entity.BankCode,
        SwiftCode = entity.SwiftCode ?? string.Empty,
        Address = entity.Address ?? string.Empty,
        City = entity.City ?? string.Empty,
        State = entity.State ?? string.Empty,
        Country = entity.Country ?? string.Empty,
        PostalCode = entity.PostalCode ?? string.Empty,
        Phone = entity.Phone ?? string.Empty,
        Email = entity.Email ?? string.Empty,
        Website = entity.Website ?? string.Empty,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy
    };

    private static BankAccountDto MapAccount(BankAccount entity) => new()
    {
        Id = entity.Id,
        BankAccountId = entity.Id,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        BankId = entity.BankId,
        AccountName = entity.AccountName,
        AccountNumber = entity.AccountNumber,
        AccountType = entity.AccountType,
        Currency = entity.Currency,
        CurrencyCode = entity.Currency,
        Balance = entity.Balance,
        IBAN = entity.IBAN,
        SWIFT = entity.SWIFT,
        GLAccountId = entity.GLAccountId,
        GLAccountCode = entity.GLAccount?.AccountCode,
        GLAccountName = entity.GLAccount?.AccountName,
        BankName = entity.Bank?.BankName ?? string.Empty,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy
    };

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
