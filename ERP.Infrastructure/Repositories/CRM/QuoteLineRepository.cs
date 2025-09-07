using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class QuoteLineRepository : IQuoteLineRepository
{
    private readonly ERPDbContext _context;

    public QuoteLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<QuoteLine?> GetByIdAsync(int id)
    {
        return await _context.QuoteLines.FindAsync(id);
    }

    public async Task<IEnumerable<QuoteLine>> GetAllAsync()
    {
        return await _context.QuoteLines.ToListAsync();
    }

    public async Task<QuoteLine> AddAsync(QuoteLine entity)
    {
        _context.QuoteLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(QuoteLine entity)
    {
        _context.QuoteLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.QuoteLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}