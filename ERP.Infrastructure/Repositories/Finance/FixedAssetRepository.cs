using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class FixedAssetRepository : IFixedAssetRepository
{
    private readonly ERPDbContext _context;

    public FixedAssetRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<FixedAsset?> GetByIdAsync(Guid id)
    {
        return await _context.FixedAssets
            .Include(f => f.GLAccount)
            .Include(f => f.DepreciationEntries)
            .FirstOrDefaultAsync(f => f.AssetId == id);
    }

    public async Task<IEnumerable<FixedAsset>> GetAllAsync()
    {
        return await _context.FixedAssets
            .Include(f => f.GLAccount)
            .Include(f => f.DepreciationEntries)
            .ToListAsync();
    }

    public async Task<IEnumerable<FixedAsset>> GetActiveAsync()
    {
        return await _context.FixedAssets
            .Include(f => f.GLAccount)
            .Include(f => f.DepreciationEntries)
            .Where(f => f.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<FixedAsset>> GetByStatusAsync(string status)
    {
        return await _context.FixedAssets
            .Include(f => f.GLAccount)
            .Include(f => f.DepreciationEntries)
            .Where(f => f.Status == status)
            .ToListAsync();
    }

    public async Task<FixedAsset?> GetByCodeAsync(string assetCode)
    {
        return await _context.FixedAssets
            .Include(f => f.GLAccount)
            .Include(f => f.DepreciationEntries)
            .FirstOrDefaultAsync(f => f.AssetCode == assetCode);
    }

    public async Task<Guid> CreateAsync(FixedAsset fixedAsset)
    {
        _context.FixedAssets.Add(fixedAsset);
        await _context.SaveChangesAsync();
        return fixedAsset.AssetId;
    }

    public async Task<bool> UpdateAsync(FixedAsset fixedAsset)
    {
        _context.FixedAssets.Update(fixedAsset);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var fixedAsset = await _context.FixedAssets.FindAsync(id);
        if (fixedAsset == null) return false;

        _context.FixedAssets.Remove(fixedAsset);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}