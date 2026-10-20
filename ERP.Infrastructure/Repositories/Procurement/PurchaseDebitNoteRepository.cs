using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseDebitNoteRepository : IPurchaseDebitNoteRepository
{
    private readonly ERPDbContext _context;

    public PurchaseDebitNoteRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseDebitNote?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.PurchaseDebitNotes
            .FirstOrDefaultAsync(dn => dn.Id == id && dn.TenantId == tenantId);
    }

    public async Task<IEnumerable<PurchaseDebitNote>> GetAllAsync(Guid tenantId)
    {
        return await _context.PurchaseDebitNotes
            .Where(dn => dn.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseDebitNote>> GetByVendorAsync(Guid vendorId, Guid tenantId)
    {
        return await _context.PurchaseDebitNotes
            .Where(dn => dn.VendorId == vendorId && dn.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseDebitNote>> GetByStatusAsync(string status, Guid tenantId)
    {
        return await _context.PurchaseDebitNotes
            .Where(dn => dn.Status == status && dn.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<PurchaseDebitNote> AddAsync(PurchaseDebitNote entity)
    {
        _context.PurchaseDebitNotes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(PurchaseDebitNote entity)
    {
        _context.PurchaseDebitNotes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.PurchaseDebitNotes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
