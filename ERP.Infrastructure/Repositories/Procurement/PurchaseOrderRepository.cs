using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ERPDbContext _context;

    public PurchaseOrderRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
    {
        return await _context.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Details)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByVendorAsync(Guid vendorId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Details)
            .Where(po => po.VendorId == vendorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(string status)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Details)
            .Where(po => po.Status == status)
            .ToListAsync();
    }

    public async Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber);
    }

    public async Task<Guid> CreateAsync(PurchaseOrder purchaseOrder)
    {
        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();
        return purchaseOrder.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseOrder purchaseOrder)
    {
        _context.PurchaseOrders.Update(purchaseOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
        if (purchaseOrder == null) return false;

        _context.PurchaseOrders.Remove(purchaseOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}