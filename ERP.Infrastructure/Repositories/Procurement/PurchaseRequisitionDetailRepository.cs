using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseRequisitionDetailRepository : IPurchaseRequisitionDetailRepository
{
    private readonly ERPDbContext _context;

    public PurchaseRequisitionDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseRequisitionDetail?> GetByIdAsync(int id)
    {
        return await _context.PurchaseRequisitionDetails.FindAsync(id);
    }

    public async Task<IEnumerable<PurchaseRequisitionDetail>> GetAllAsync()
    {
        return await _context.PurchaseRequisitionDetails.ToListAsync();
    }

    public async Task<PurchaseRequisitionDetail> AddAsync(PurchaseRequisitionDetail entity)
    {
        _context.PurchaseRequisitionDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(PurchaseRequisitionDetail entity)
    {
        _context.PurchaseRequisitionDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.PurchaseRequisitionDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}