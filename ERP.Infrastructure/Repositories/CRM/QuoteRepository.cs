using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class QuoteRepository : IQuoteRepository
{
    private readonly ERPDbContext _context;

    public QuoteRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Quote?> GetByIdAsync(Guid id)
    {
        return await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quote>> GetAllAsync()
    {
        return await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
            .Where(q => q.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quote>> GetByStatusAsync(string status)
    {
        return await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
            .Where(q => q.Status == status)
            .ToListAsync();
    }

    public async Task<Quote?> GetByNumberAsync(string quoteNumber)
    {
        return await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
            .FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber);
    }

    public async Task<Guid> CreateAsync(Quote quote)
    {
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();
        return quote.Id;
    }

    public async Task<bool> UpdateAsync(Quote quote)
    {
        _context.Quotes.Update(quote);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null) return false;

        _context.Quotes.Remove(quote);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}