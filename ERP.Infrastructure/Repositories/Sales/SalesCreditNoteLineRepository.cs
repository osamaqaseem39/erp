using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class SalesCreditNoteLineRepository : ISalesCreditNoteLineRepository
{
    private readonly ERPDbContext _context;

    public SalesCreditNoteLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesCreditNoteLine>> GetByCreditNoteIdAsync(Guid creditNoteId, Guid tenantId)
    {
        return await _context.SalesCreditNoteLines
            .Where(l => l.CreditNoteId == creditNoteId && l.TenantId == tenantId)
            .OrderBy(l => l.LineNumber)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SalesCreditNoteLine line)
    {
        _context.SalesCreditNoteLines.Add(line);
        await _context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteByCreditNoteIdAsync(Guid creditNoteId, Guid tenantId)
    {
        var lines = await _context.SalesCreditNoteLines
            .Where(l => l.CreditNoteId == creditNoteId && l.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;

        _context.SalesCreditNoteLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
        return true;
    }
}
