using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
{
    private readonly ERPDbContext _context;

    public PurchaseInvoiceRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseInvoice?> GetByIdAsync(Guid id)
    {
        return await _context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.Details)
            .FirstOrDefaultAsync(pi => pi.Id == id);
    }

    public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
    {
        return await _context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.Details)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseInvoice>> GetByVendorAsync(Guid vendorId)
    {
        return await _context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.Details)
            .Where(pi => pi.VendorId == vendorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PurchaseInvoice>> GetByStatusAsync(string status)
    {
        return await _context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.Details)
            .Where(pi => pi.Status == status)
            .ToListAsync();
    }

    public async Task<PurchaseInvoice?> GetByNumberAsync(string invoiceNumber)
    {
        return await _context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.Details)
            .FirstOrDefaultAsync(pi => pi.InvoiceNumber == invoiceNumber);
    }

    public async Task<Guid> CreateAsync(PurchaseInvoice purchaseInvoice)
    {
        _context.PurchaseInvoices.Add(purchaseInvoice);
        await _context.SaveChangesAsync();
        return purchaseInvoice.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseInvoice purchaseInvoice)
    {
        _context.PurchaseInvoices.Update(purchaseInvoice);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var purchaseInvoice = await _context.PurchaseInvoices.FindAsync(id);
        if (purchaseInvoice == null) return false;

        _context.PurchaseInvoices.Remove(purchaseInvoice);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}