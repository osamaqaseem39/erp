using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseRequisitionRepository : IPurchaseRequisitionRepository
{
    private readonly ERPDbContext _context;

    public PurchaseRequisitionRepository(ERPDbContext context) => _context = context;

    public async Task<PurchaseRequisition?> GetByIdAsync(Guid id, Guid tenantId) =>
        await _context.PurchaseRequisitions.Include(pr => pr.Details)
            .FirstOrDefaultAsync(pr => pr.Id == id && pr.TenantId == tenantId);

    public async Task<IEnumerable<PurchaseRequisition>> GetAllAsync(Guid tenantId) =>
        await _context.PurchaseRequisitions.Include(pr => pr.Details)
            .Where(pr => pr.TenantId == tenantId).OrderByDescending(pr => pr.RequestDate).ToListAsync();

    public async Task<IEnumerable<PurchaseRequisition>> GetByStatusAsync(string status, Guid tenantId) =>
        await _context.PurchaseRequisitions.Include(pr => pr.Details)
            .Where(pr => pr.Status == status && pr.TenantId == tenantId).ToListAsync();

    public async Task<IEnumerable<PurchaseRequisition>> GetByRequestorAsync(Guid requestorId, Guid tenantId) =>
        await _context.PurchaseRequisitions.Include(pr => pr.Details)
            .Where(pr => (pr.RequestorId == requestorId || pr.RequestedByUserId == requestorId) && pr.TenantId == tenantId)
            .ToListAsync();

    public async Task<IEnumerable<PurchaseRequisition>> GetByDateRangeAsync(DateTime start, DateTime end, Guid tenantId) =>
        await _context.PurchaseRequisitions.Include(pr => pr.Details)
            .Where(pr => pr.TenantId == tenantId && pr.RequestDate >= start && pr.RequestDate <= end).ToListAsync();

    public async Task<Guid> CreateAsync(PurchaseRequisition purchaseRequisition)
    {
        _context.PurchaseRequisitions.Add(purchaseRequisition);
        await _context.SaveChangesAsync();
        return purchaseRequisition.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseRequisition purchaseRequisition)
    {
        _context.PurchaseRequisitions.Update(purchaseRequisition);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.PurchaseRequisitions.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}
