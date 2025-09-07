using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class CostCenterRepository : ICostCenterRepository
{
    private readonly ERPDbContext _context;

    public CostCenterRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<CostCenter?> GetByIdAsync(Guid id)
    {
        return await _context.CostCenters.FindAsync(id);
    }

    public async Task<IEnumerable<CostCenter>> GetAllAsync()
    {
        return await _context.CostCenters.ToListAsync();
    }

    public async Task<IEnumerable<CostCenter>> GetActiveAsync()
    {
        return await _context.CostCenters
            .Where(cc => cc.IsActive)
            .ToListAsync();
    }

    public async Task<CostCenter?> GetByCodeAsync(string costCenterCode)
    {
        return await _context.CostCenters
            .FirstOrDefaultAsync(cc => cc.CostCenterCode == costCenterCode);
    }

    public async Task<Guid> CreateAsync(CostCenter costCenter)
    {
        _context.CostCenters.Add(costCenter);
        await _context.SaveChangesAsync();
        return costCenter.CostCenterId;
    }

    public async Task<bool> UpdateAsync(CostCenter costCenter)
    {
        _context.CostCenters.Update(costCenter);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var costCenter = await _context.CostCenters.FindAsync(id);
        if (costCenter == null) return false;

        _context.CostCenters.Remove(costCenter);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}