using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ERPDbContext _context;

    public ExchangeRateRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        return await _context.ExchangeRates
            .Include(e => e.FromCurrencyNavigation)
            .Include(e => e.ToCurrencyNavigation)
            .FirstOrDefaultAsync(e => e.ExchangeRateId == id);
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllAsync()
    {
        return await _context.ExchangeRates
            .Include(e => e.FromCurrencyNavigation)
            .Include(e => e.ToCurrencyNavigation)
            .OrderByDescending(e => e.EffectiveDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetByCurrencyAsync(string fromCurrency, string toCurrency)
    {
        return await _context.ExchangeRates
            .Include(e => e.FromCurrencyNavigation)
            .Include(e => e.ToCurrencyNavigation)
            .Where(e => e.FromCurrency == fromCurrency && e.ToCurrency == toCurrency)
            .OrderByDescending(e => e.EffectiveDate)
            .ToListAsync();
    }

    public async Task<ExchangeRate?> GetCurrentRateAsync(string fromCurrency, string toCurrency)
    {
        var today = DateTime.Today;
        return await _context.ExchangeRates
            .Include(e => e.FromCurrencyNavigation)
            .Include(e => e.ToCurrencyNavigation)
            .Where(e => e.FromCurrency == fromCurrency && 
                       e.ToCurrency == toCurrency && 
                       e.EffectiveDate <= today)
            .OrderByDescending(e => e.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    public async Task<ExchangeRate> CreateAsync(ExchangeRate entity)
    {
        _context.ExchangeRates.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ExchangeRate> UpdateAsync(ExchangeRate entity)
    {
        _context.ExchangeRates.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        _context.ExchangeRates.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}