using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class POSTransactionLineRepository : IPOSTransactionLineRepository
{
    private readonly ERPDbContext _context;

    public POSTransactionLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<POSTransactionLine>> GetByTransactionIdAsync(Guid transactionId, Guid tenantId)
    {
        return await _context.POSTransactionLines
            .Where(l => l.POSTransactionId == transactionId && l.TenantId == tenantId)
            .OrderBy(l => l.LineNumber)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(POSTransactionLine line)
    {
        _context.POSTransactionLines.Add(line);
        await _context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteByTransactionIdAsync(Guid transactionId, Guid tenantId)
    {
        var lines = await _context.POSTransactionLines
            .Where(l => l.POSTransactionId == transactionId && l.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;

        _context.POSTransactionLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
        return true;
    }
}
