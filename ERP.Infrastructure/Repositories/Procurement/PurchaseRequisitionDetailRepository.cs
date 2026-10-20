using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseRequisitionDetailRepository : IPurchaseRequisitionDetailRepository
{
    private readonly ERPDbContext _context;

    public PurchaseRequisitionDetailRepository(ERPDbContext context) => _context = context;

    public async Task<IEnumerable<PurchaseRequisitionDetail>> GetByRequisitionIdAsync(Guid prId, Guid tenantId) =>
        await _context.PurchaseRequisitionDetails.Where(d => d.PRId == prId && d.TenantId == tenantId).ToListAsync();

    public async Task<Guid> CreateAsync(PurchaseRequisitionDetail detail)
    {
        _context.PurchaseRequisitionDetails.Add(detail);
        await _context.SaveChangesAsync();
        return detail.Id;
    }

    public async Task<bool> DeleteByRequisitionIdAsync(Guid prId, Guid tenantId)
    {
        var rows = await _context.PurchaseRequisitionDetails.Where(d => d.PRId == prId && d.TenantId == tenantId).ToListAsync();
        if (rows.Count == 0) return true;
        _context.PurchaseRequisitionDetails.RemoveRange(rows);
        await _context.SaveChangesAsync();
        return true;
    }
}
