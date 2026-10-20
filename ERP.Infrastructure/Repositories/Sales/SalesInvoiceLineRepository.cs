using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class SalesInvoiceLineRepository : ISalesInvoiceLineRepository
{
    private readonly ERPDbContext _context;

    public SalesInvoiceLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesInvoiceLine>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
    {
        return await _context.SalesInvoiceLines
            .Where(l => l.SalesInvoiceId == invoiceId && l.TenantId == tenantId)
            .OrderBy(l => l.LineNumber)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SalesInvoiceLine line)
    {
        _context.SalesInvoiceLines.Add(line);
        await _context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
    {
        var lines = await _context.SalesInvoiceLines
            .Where(l => l.SalesInvoiceId == invoiceId && l.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;

        _context.SalesInvoiceLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
        return true;
    }
}
