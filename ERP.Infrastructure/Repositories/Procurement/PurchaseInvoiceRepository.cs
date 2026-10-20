using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
{
    private readonly ERPDbContext _context;

    public PurchaseInvoiceRepository(ERPDbContext context) => _context = context;

    public async Task<PurchaseInvoice?> GetByIdAsync(Guid id, Guid tenantId) =>
        await _context.PurchaseInvoices.Include(i => i.Details)
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);

    public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync(Guid tenantId) =>
        await _context.PurchaseInvoices.Include(i => i.Details)
            .Where(i => i.TenantId == tenantId).OrderByDescending(i => i.InvoiceDate).ToListAsync();

    public async Task<IEnumerable<PurchaseInvoice>> GetByVendorAsync(Guid vendorId, Guid tenantId) =>
        await _context.PurchaseInvoices.Include(i => i.Details)
            .Where(i => i.VendorId == vendorId && i.TenantId == tenantId).ToListAsync();

    public async Task<IEnumerable<PurchaseInvoice>> GetByStatusAsync(string status, Guid tenantId) =>
        await _context.PurchaseInvoices.Include(i => i.Details)
            .Where(i => i.Status == status && i.TenantId == tenantId).ToListAsync();

    public async Task<IEnumerable<PurchaseInvoice>> GetByDateRangeAsync(DateTime start, DateTime end, Guid tenantId) =>
        await _context.PurchaseInvoices.Include(i => i.Details)
            .Where(i => i.TenantId == tenantId && i.InvoiceDate >= start && i.InvoiceDate <= end).ToListAsync();

    public async Task<Guid> CreateAsync(PurchaseInvoice purchaseInvoice)
    {
        _context.PurchaseInvoices.Add(purchaseInvoice);
        await _context.SaveChangesAsync();
        return purchaseInvoice.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseInvoice purchaseInvoice)
    {
        _context.PurchaseInvoices.Update(purchaseInvoice);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.PurchaseInvoices.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}