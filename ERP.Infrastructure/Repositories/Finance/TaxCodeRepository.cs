using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class TaxCodeRepository : ITaxCodeRepository
{
    private readonly ERPDbContext _context;

    public TaxCodeRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<TaxCode?> GetByIdAsync(Guid id)
    {
        return await _context.TaxCodes
            .Include(tc => tc.Account)
            .FirstOrDefaultAsync(tc => tc.TaxCodeId == id);
    }

    public async Task<IEnumerable<TaxCode>> GetAllAsync()
    {
        return await _context.TaxCodes
            .Include(tc => tc.Account)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaxCode>> GetActiveAsync()
    {
        return await _context.TaxCodes
            .Include(tc => tc.Account)
            .Where(tc => tc.IsActive)
            .ToListAsync();
    }

    public async Task<TaxCode?> GetByCodeAsync(string taxCode)
    {
        return await _context.TaxCodes
            .Include(tc => tc.Account)
            .FirstOrDefaultAsync(tc => tc.Code == taxCode);
    }

    public async Task<Guid> CreateAsync(TaxCode taxCode)
    {
        _context.TaxCodes.Add(taxCode);
        await _context.SaveChangesAsync();
        return taxCode.Id;
    }

    public async Task<bool> UpdateAsync(TaxCode taxCode)
    {
        _context.TaxCodes.Update(taxCode);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var taxCode = await _context.TaxCodes.FindAsync(id);
        if (taxCode == null) return false;

        _context.TaxCodes.Remove(taxCode);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}