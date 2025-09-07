using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class InvoiceLineRepository : IInvoiceLineRepository
{
    private readonly ERPDbContext _context;

    public InvoiceLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceLine?> GetByIdAsync(int id)
    {
        return await _context.InvoiceLines.FindAsync(id);
    }

    public async Task<IEnumerable<InvoiceLine>> GetAllAsync()
    {
        return await _context.InvoiceLines.ToListAsync();
    }

    public async Task<InvoiceLine> AddAsync(InvoiceLine entity)
    {
        _context.InvoiceLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(InvoiceLine entity)
    {
        _context.InvoiceLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.InvoiceLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}