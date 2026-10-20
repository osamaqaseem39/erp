using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class SalesCreditNoteRepository : ISalesCreditNoteRepository
{
    private readonly ERPDbContext _context;

    public SalesCreditNoteRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SalesCreditNote?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.SalesCreditNotes
            .FirstOrDefaultAsync(cn => cn.Id == id && cn.TenantId == tenantId);
    }

    public async Task<IEnumerable<SalesCreditNote>> GetAllAsync(Guid tenantId)
    {
        return await _context.SalesCreditNotes
            .Where(cn => cn.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesCreditNote>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        return await _context.SalesCreditNotes
            .Where(cn => cn.CustomerId == customerId && cn.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesCreditNote>> GetByStatusAsync(string status, Guid tenantId)
    {
        return await _context.SalesCreditNotes
            .Where(cn => cn.Status == status && cn.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<SalesCreditNote> AddAsync(SalesCreditNote entity)
    {
        _context.SalesCreditNotes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(SalesCreditNote entity)
    {
        _context.SalesCreditNotes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.SalesCreditNotes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
