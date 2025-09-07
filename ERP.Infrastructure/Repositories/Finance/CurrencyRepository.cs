using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ERPDbContext _context;

    public CurrencyRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Currency?> GetByIdAsync(string currencyCode)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode);
    }

    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _context.Currencies
            .OrderBy(c => c.CurrencyCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<Currency>> GetActiveAsync()
    {
        return await _context.Currencies
            .Where(c => c.IsActive)
            .OrderBy(c => c.CurrencyCode)
            .ToListAsync();
    }

    public async Task<Currency> CreateAsync(Currency entity)
    {
        _context.Currencies.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Currency> UpdateAsync(Currency entity)
    {
        _context.Currencies.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(string currencyCode)
    {
        var entity = await GetByIdAsync(currencyCode);
        if (entity == null)
            return false;

        _context.Currencies.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string currencyCode)
    {
        return await _context.Currencies
            .AnyAsync(c => c.CurrencyCode == currencyCode);
    }
}