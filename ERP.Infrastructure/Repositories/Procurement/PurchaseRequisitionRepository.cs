using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseRequisitionRepository : IPurchaseRequisitionRepository
{
    private readonly ERPDbContext _context;

    public PurchaseRequisitionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseRequisition?> GetByIdAsync(Guid id)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.Details)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<IEnumerable<PurchaseRequisition>> GetAllAsync()
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.Details)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseRequisition>> GetByStatusAsync(string status)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.Details)
            .Where(pr => pr.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseRequisition>> GetByRequestorAsync(Guid requestorId)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.Details)
            .Where(pr => pr.RequestorId == requestorId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PurchaseRequisition purchaseRequisition)
    {
        _context.PurchaseRequisitions.Add(purchaseRequisition);
        await _context.SaveChangesAsync();
        return purchaseRequisition.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseRequisition purchaseRequisition)
    {
        _context.PurchaseRequisitions.Update(purchaseRequisition);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var purchaseRequisition = await _context.PurchaseRequisitions.FindAsync(id);
        if (purchaseRequisition == null) return false;

        _context.PurchaseRequisitions.Remove(purchaseRequisition);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}