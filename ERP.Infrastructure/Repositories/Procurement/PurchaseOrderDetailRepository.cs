using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseOrderDetailRepository : IPurchaseOrderDetailRepository
{
    private readonly ERPDbContext _context;

    public PurchaseOrderDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrderDetail?> GetByIdAsync(int id)
    {
        return await _context.PurchaseOrderDetails.FindAsync(id);
    }

    public async Task<IEnumerable<PurchaseOrderDetail>> GetAllAsync()
    {
        return await _context.PurchaseOrderDetails.ToListAsync();
    }

    public async Task<PurchaseOrderDetail> AddAsync(PurchaseOrderDetail entity)
    {
        _context.PurchaseOrderDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(PurchaseOrderDetail entity)
    {
        _context.PurchaseOrderDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.PurchaseOrderDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}