using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class VendorBankAccountRepository : IVendorBankAccountRepository
{
    private readonly ERPDbContext _context;

    public VendorBankAccountRepository(ERPDbContext context) => _context = context;

    private IQueryable<VendorBankAccount> Query(Guid tenantId) =>
        _context.VendorBankAccounts.Where(a => a.TenantId == tenantId);

    public async Task<VendorBankAccount?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<VendorBankAccount>> GetByVendorAsync(Guid vendorId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<VendorBankAccount>();
        return await Query(tenantId)
            .Where(a => a.VendorId == vendorId && a.IsActive)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.BankName)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(VendorBankAccount entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _context.VendorBankAccounts.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(VendorBankAccount entity)
    {
        _context.VendorBankAccounts.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
}
