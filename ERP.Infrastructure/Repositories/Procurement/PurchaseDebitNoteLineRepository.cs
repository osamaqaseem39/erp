using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseDebitNoteLineRepository : IPurchaseDebitNoteLineRepository
{
    private readonly ERPDbContext _context;

    public PurchaseDebitNoteLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseDebitNoteLine>> GetByDebitNoteIdAsync(Guid debitNoteId, Guid tenantId)
    {
        return await _context.PurchaseDebitNoteLines
            .Where(l => l.DebitNoteId == debitNoteId && l.TenantId == tenantId)
            .OrderBy(l => l.LineNumber)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PurchaseDebitNoteLine line)
    {
        _context.PurchaseDebitNoteLines.Add(line);
        await _context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteByDebitNoteIdAsync(Guid debitNoteId, Guid tenantId)
    {
        var lines = await _context.PurchaseDebitNoteLines
            .Where(l => l.DebitNoteId == debitNoteId && l.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;

        _context.PurchaseDebitNoteLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
        return true;
    }
}
