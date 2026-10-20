using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class AccountDefaultService : IAccountDefaultService
{
    private readonly ERPDbContext _db;
    private readonly IChartOfAccountRepository _accounts;

    public AccountDefaultService(ERPDbContext db, IChartOfAccountRepository accounts)
    {
        _db = db;
        _accounts = accounts;
    }

    public async Task<IEnumerable<AccountDefaultDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<AccountDefaultDto>();
        var list = await _db.AccountDefaults.Include(d => d.Account)
            .Where(d => d.TenantId == tenantId)
            .OrderBy(d => d.Purpose)
            .ToListAsync();
        return list.Select(Map);
    }

    public async Task<AccountDefaultDto?> GetByPurposeAsync(string purpose, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _db.AccountDefaults.Include(d => d.Account)
            .Where(d => d.TenantId == tenantId && d.Purpose == purpose && d.IsActive)
            .FirstOrDefaultAsync();
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<AccountDefaultDto>> UpsertAsync(UpsertAccountDefaultsDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<AccountDefaultDto>();
        foreach (var item in dto.Defaults)
        {
            if (string.IsNullOrWhiteSpace(item.Purpose) || item.AccountId == Guid.Empty)
                continue;
            if (!AccountPurposes.All.Contains(item.Purpose, StringComparer.OrdinalIgnoreCase))
                continue;

            var account = await _accounts.GetByIdAsync(item.AccountId, tenantId);
            if (account == null || !account.IsPostingAllowed || !account.IsActive)
                continue;

            var existing = await _db.AccountDefaults
                .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Purpose == item.Purpose);

            if (existing == null)
            {
                var id = Guid.NewGuid();
                _db.AccountDefaults.Add(new AccountDefault
                {
                    Id = id,
                    AccountDefaultId = id,
                    TenantId = tenantId,
                    Purpose = item.Purpose,
                    AccountId = item.AccountId,
                    IsActive = item.IsActive,
                    CreatedBy = userId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.AccountId = item.AccountId;
                existing.IsActive = item.IsActive;
                existing.UpdatedBy = userId.ToString();
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return await GetAllAsync(tenantId);
    }

    private static AccountDefaultDto Map(AccountDefault e) => new()
    {
        Id = e.Id,
        AccountDefaultId = e.AccountDefaultId,
        TenantId = e.TenantId,
        Purpose = e.Purpose,
        AccountId = e.AccountId,
        AccountCode = e.Account?.AccountCode,
        AccountName = e.Account?.AccountName,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
