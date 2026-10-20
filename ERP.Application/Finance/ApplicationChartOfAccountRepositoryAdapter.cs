using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class ApplicationChartOfAccountRepositoryAdapter : IChartOfAccountRepository
{
    private readonly ERPDbContext _db;

    public ApplicationChartOfAccountRepositoryAdapter(ERPDbContext db) => _db = db;

    public async Task<ChartOfAccount?> GetByIdAsync(Guid accountId, Guid tenantId)
    {
        var q = _db.ChartOfAccounts.Where(c => c.AccountId == accountId || c.Id == accountId);
        if (tenantId != Guid.Empty) q = q.Where(c => c.TenantId == tenantId);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ChartOfAccount>> GetAllAsync(
        Guid tenantId, string? accountType = null, bool? isActive = null)
    {
        var q = _db.ChartOfAccounts.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(accountType)) q = q.Where(c => c.AccountType == accountType);
        if (isActive.HasValue) q = q.Where(c => c.IsActive == isActive.Value);
        return await q.OrderBy(c => c.AccountCode).ToListAsync();
    }

    public async Task<ChartOfAccount?> GetByAccountCodeAsync(string accountCode, Guid tenantId)
    {
        var q = _db.ChartOfAccounts.Where(c => c.AccountCode == accountCode);
        if (tenantId != Guid.Empty) q = q.Where(c => c.TenantId == tenantId);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ChartOfAccount>> GetChildAccountsAsync(Guid parentAccountId, Guid tenantId)
    {
        var q = _db.ChartOfAccounts.Where(c => c.ParentAccountId == parentAccountId);
        if (tenantId != Guid.Empty) q = q.Where(c => c.TenantId == tenantId);
        return await q.OrderBy(c => c.AccountCode).ToListAsync();
    }

    public async Task<ChartOfAccount> AddAsync(ChartOfAccount account)
    {
        if (account.Id == Guid.Empty) account.Id = Guid.NewGuid();
        if (account.AccountId == Guid.Empty) account.AccountId = account.Id;
        account.CreatedAt = DateTime.UtcNow;
        _db.ChartOfAccounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task<ChartOfAccount> UpdateAsync(ChartOfAccount account)
    {
        account.UpdatedAt = DateTime.UtcNow;
        _db.ChartOfAccounts.Update(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task<bool> DeleteAsync(Guid accountId, Guid tenantId)
    {
        var entity = await GetByIdAsync(accountId, tenantId);
        if (entity == null) return false;
        if (await HasChildAccountsAsync(entity.AccountId, tenantId)) return false;
        if (await HasJournalEntriesAsync(entity.AccountId, tenantId)) return false;

        _db.ChartOfAccounts.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasChildAccountsAsync(Guid accountId, Guid tenantId)
    {
        var q = _db.ChartOfAccounts.Where(c => c.ParentAccountId == accountId);
        if (tenantId != Guid.Empty) q = q.Where(c => c.TenantId == tenantId);
        return await q.AnyAsync();
    }

    public async Task<bool> HasJournalEntriesAsync(Guid accountId, Guid tenantId)
    {
        var q = _db.JournalEntries.Where(e => e.AccountId == accountId);
        if (tenantId != Guid.Empty) q = q.Where(e => e.TenantId == tenantId);
        return await q.AnyAsync();
    }

    public async Task<IEnumerable<ChartOfAccount>> GetByAccountTypeAsync(string accountType, Guid tenantId)
        => await GetAllAsync(tenantId, accountType: accountType);

    public async Task<IEnumerable<ChartOfAccount>> GetPostingAccountsAsync(Guid tenantId)
    {
        var q = _db.ChartOfAccounts.Where(c => c.IsPostingAllowed && c.IsActive);
        if (tenantId != Guid.Empty) q = q.Where(c => c.TenantId == tenantId);
        return await q.OrderBy(c => c.AccountCode).ToListAsync();
    }
}
