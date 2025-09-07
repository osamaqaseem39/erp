using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class TaxRateRepository : ITaxRateRepository
{
    private readonly ERPDbContext _context;

    public TaxRateRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<TaxRate?> GetByIdAsync(Guid id)
    {
        return await _context.TaxRates.FindAsync(id);
    }

    public async Task<IEnumerable<TaxRate>> GetAllAsync()
    {
        return await _context.TaxRates.ToListAsync();
    }

    public async Task<Guid> CreateAsync(TaxRate entity)
    {
        _context.TaxRates.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(TaxRate entity)
    {
        _context.TaxRates.Update(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.TaxRates.Remove(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }

    public async Task<IEnumerable<TaxRate>> GetActiveRatesAsync()
    {
        return await _context.TaxRates
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaxRate>> GetByTypeAsync(string taxType)
    {
        return await _context.TaxRates
            .Where(t => t.TaxType == taxType)
            .ToListAsync();
    }
}