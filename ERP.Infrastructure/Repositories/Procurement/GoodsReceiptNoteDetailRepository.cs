using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class GoodsReceiptNoteDetailRepository : IGoodsReceiptNoteDetailRepository
{
    private readonly ERPDbContext _context;

    public GoodsReceiptNoteDetailRepository(ERPDbContext context) => _context = context;

    public async Task<IEnumerable<GoodsReceiptNoteDetail>> GetByGrnIdAsync(Guid grnId, Guid tenantId) =>
        await _context.GoodsReceiptNoteDetails.Where(d => d.GRNId == grnId && d.TenantId == tenantId).ToListAsync();

    public async Task<Guid> CreateAsync(GoodsReceiptNoteDetail detail)
    {
        _context.GoodsReceiptNoteDetails.Add(detail);
        await _context.SaveChangesAsync();
        return detail.Id;
    }

    public async Task<bool> DeleteByGrnIdAsync(Guid grnId, Guid tenantId)
    {
        var rows = await _context.GoodsReceiptNoteDetails.Where(d => d.GRNId == grnId && d.TenantId == tenantId).ToListAsync();
        if (rows.Count == 0) return true;
        _context.GoodsReceiptNoteDetails.RemoveRange(rows);
        await _context.SaveChangesAsync();
        return true;
    }
}
