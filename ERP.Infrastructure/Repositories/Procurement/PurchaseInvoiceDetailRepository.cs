using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseInvoiceDetailRepository : IPurchaseInvoiceDetailRepository
{
    private readonly ERPDbContext _context;

    public PurchaseInvoiceDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseInvoiceDetail?> GetByIdAsync(int id)
    {
        return await _context.PurchaseInvoiceDetails.FindAsync(id);
    }

    public async Task<IEnumerable<PurchaseInvoiceDetail>> GetAllAsync()
    {
        return await _context.PurchaseInvoiceDetails.ToListAsync();
    }

    public async Task<PurchaseInvoiceDetail> AddAsync(PurchaseInvoiceDetail entity)
    {
        _context.PurchaseInvoiceDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(PurchaseInvoiceDetail entity)
    {
        _context.PurchaseInvoiceDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.PurchaseInvoiceDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}