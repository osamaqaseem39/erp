using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class SalesInvoiceRepository : ISalesInvoiceRepository
{
    private readonly ERPDbContext _context;

    public SalesInvoiceRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SalesInvoice?> GetByIdAsync(int id)
    {
        return await _context.SalesInvoices.FindAsync(id);
    }

    public async Task<IEnumerable<SalesInvoice>> GetAllAsync()
    {
        return await _context.SalesInvoices.ToListAsync();
    }

    public async Task<SalesInvoice> AddAsync(SalesInvoice entity)
    {
        _context.SalesInvoices.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(SalesInvoice entity)
    {
        _context.SalesInvoices.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.SalesInvoices.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}