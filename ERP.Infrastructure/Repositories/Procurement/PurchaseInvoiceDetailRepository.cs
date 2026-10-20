using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseInvoiceDetailRepository : IPurchaseInvoiceDetailRepository
{
    private readonly ERPDbContext _context;

    public PurchaseInvoiceDetailRepository(ERPDbContext context) => _context = context;

    public async Task<IEnumerable<PurchaseInvoiceDetail>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId) =>
        await _context.PurchaseInvoiceDetails.Where(d => d.InvoiceId == invoiceId && d.TenantId == tenantId).ToListAsync();

    public async Task<Guid> CreateAsync(PurchaseInvoiceDetail detail)
    {
        _context.PurchaseInvoiceDetails.Add(detail);
        await _context.SaveChangesAsync();
        return detail.Id;
    }

    public async Task<bool> DeleteByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
    {
        var rows = await _context.PurchaseInvoiceDetails.Where(d => d.InvoiceId == invoiceId && d.TenantId == tenantId).ToListAsync();
        if (rows.Count == 0) return true;
        _context.PurchaseInvoiceDetails.RemoveRange(rows);
        await _context.SaveChangesAsync();
        return true;
    }
}
