using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class SalesInvoiceLineRepository : ISalesInvoiceLineRepository
{
    private readonly ERPDbContext _context;

    public SalesInvoiceLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SalesInvoiceLine?> GetByIdAsync(int id)
    {
        return await _context.SalesInvoiceLines.FindAsync(id);
    }

    public async Task<IEnumerable<SalesInvoiceLine>> GetAllAsync()
    {
        return await _context.SalesInvoiceLines.ToListAsync();
    }

    public async Task<SalesInvoiceLine> AddAsync(SalesInvoiceLine entity)
    {
        _context.SalesInvoiceLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(SalesInvoiceLine entity)
    {
        _context.SalesInvoiceLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.SalesInvoiceLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}