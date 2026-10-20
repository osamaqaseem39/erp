using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class LandedCostVoucherRepository : ILandedCostVoucherRepository
{
    private readonly ERPDbContext _db;
    public LandedCostVoucherRepository(ERPDbContext db) => _db = db;

    public Task<LandedCostVoucher?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.LandedCostVouchers.Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);

    public async Task<IEnumerable<LandedCostVoucher>> GetAllAsync(Guid tenantId) =>
        await _db.LandedCostVouchers.Include(v => v.Lines)
            .Where(v => v.TenantId == tenantId)
            .OrderByDescending(v => v.VoucherDate)
            .ToListAsync();

    public async Task<Guid> CreateAsync(LandedCostVoucher entity)
    {
        _db.LandedCostVouchers.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(LandedCostVoucher entity)
    {
        _db.LandedCostVouchers.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.LandedCostVouchers.Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        _db.LandedCostVoucherLines.RemoveRange(entity.Lines);
        _db.LandedCostVouchers.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class VendorContractRepository : IVendorContractRepository
{
    private readonly ERPDbContext _db;
    public VendorContractRepository(ERPDbContext db) => _db = db;

    public Task<VendorContract?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.VendorContracts.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

    public async Task<IEnumerable<VendorContract>> GetByVendorAsync(Guid vendorId, Guid tenantId) =>
        await _db.VendorContracts
            .Where(c => c.TenantId == tenantId && c.VendorId == vendorId && c.IsActive)
            .OrderByDescending(c => c.ValidFrom)
            .ToListAsync();

    public async Task<IEnumerable<VendorContract>> GetActiveForVendorItemAsync(Guid vendorId, Guid? itemId, DateTime asOf, Guid tenantId)
    {
        var q = _db.VendorContracts.Where(c =>
            c.TenantId == tenantId
            && c.VendorId == vendorId
            && c.IsActive
            && c.ValidFrom <= asOf
            && (c.ValidTo == null || c.ValidTo >= asOf));

        if (itemId.HasValue && itemId != Guid.Empty)
            q = q.Where(c => c.ItemId == null || c.ItemId == itemId);

        return await q.OrderByDescending(c => c.ItemId != null).ThenByDescending(c => c.ValidFrom).ToListAsync();
    }

    public async Task<VendorContract> CreateAsync(VendorContract entity)
    {
        _db.VendorContracts.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(VendorContract entity)
    {
        _db.VendorContracts.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.VendorContracts.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
