using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ERPDbContext _context;

    public InvoiceRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLines)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<Guid> CreateAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice.InvoiceId;
    }

    public async Task<bool> UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return false;

        _context.Invoices.Remove(invoice);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}