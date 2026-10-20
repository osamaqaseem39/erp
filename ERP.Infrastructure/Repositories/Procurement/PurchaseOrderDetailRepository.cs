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

    public async Task<IEnumerable<PurchaseOrderDetail>> GetByPurchaseOrderIdAsync(Guid poId, Guid tenantId)
    {
        return await _context.PurchaseOrderDetails
            .Where(d => d.POId == poId && d.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PurchaseOrderDetail detail)
    {
        _context.PurchaseOrderDetails.Add(detail);
        await _context.SaveChangesAsync();
        return detail.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseOrderDetail detail)
    {
        _context.PurchaseOrderDetails.Update(detail);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByPurchaseOrderIdAsync(Guid poId, Guid tenantId)
    {
        var lines = await _context.PurchaseOrderDetails
            .Where(d => d.POId == poId && d.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;

        _context.PurchaseOrderDetails.RemoveRange(lines);
        await _context.SaveChangesAsync();
        return true;
    }
}
