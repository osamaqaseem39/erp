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

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, Guid tenantId)
    {
        // Details only — do not Include Vendor (required nav → inner join drops orphans).
        return await _context.PurchaseOrders
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(Guid tenantId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Details)
            .Where(po => po.TenantId == tenantId)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByVendorAsync(Guid vendorId, Guid tenantId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Details)
            .Where(po => po.VendorId == vendorId && po.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(string status, Guid tenantId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Details)
            .Where(po => po.Status == status && po.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Details)
            .Where(po => po.TenantId == tenantId && po.OrderDate >= startDate && po.OrderDate <= endDate)
            .ToListAsync();
    }

    public async Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber, Guid tenantId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber && po.TenantId == tenantId);
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
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var purchaseOrder = await GetByIdAsync(id, tenantId);
        if (purchaseOrder == null) return false;

        _context.PurchaseOrders.Remove(purchaseOrder);
        return await _context.SaveChangesAsync() > 0;
    }
}
