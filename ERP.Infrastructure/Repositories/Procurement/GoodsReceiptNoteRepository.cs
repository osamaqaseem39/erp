using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class GoodsReceiptNoteRepository : IGoodsReceiptNoteRepository
{
    private readonly ERPDbContext _context;

    public GoodsReceiptNoteRepository(ERPDbContext context) => _context = context;

    public async Task<GoodsReceiptNote?> GetByIdAsync(Guid id, Guid tenantId) =>
        await _context.GoodsReceiptNotes.Include(g => g.Details)
            .FirstOrDefaultAsync(g => g.Id == id && g.TenantId == tenantId);

    public async Task<IEnumerable<GoodsReceiptNote>> GetAllAsync(Guid tenantId) =>
        await _context.GoodsReceiptNotes.Include(g => g.Details)
            .Where(g => g.TenantId == tenantId).OrderByDescending(g => g.ReceivedDate).ToListAsync();

    public async Task<IEnumerable<GoodsReceiptNote>> GetByPurchaseOrderAsync(Guid poId, Guid tenantId) =>
        await _context.GoodsReceiptNotes.Include(g => g.Details)
            .Where(g => g.POId == poId && g.TenantId == tenantId).ToListAsync();

    public async Task<IEnumerable<GoodsReceiptNote>> GetByStatusAsync(string status, Guid tenantId) =>
        await _context.GoodsReceiptNotes.Include(g => g.Details)
            .Where(g => g.Status == status && g.TenantId == tenantId).ToListAsync();

    public async Task<IEnumerable<GoodsReceiptNote>> GetByDateRangeAsync(DateTime start, DateTime end, Guid tenantId) =>
        await _context.GoodsReceiptNotes.Include(g => g.Details)
            .Where(g => g.TenantId == tenantId && g.ReceivedDate >= start && g.ReceivedDate <= end).ToListAsync();

    public async Task<Guid> CreateAsync(GoodsReceiptNote goodsReceiptNote)
    {
        _context.GoodsReceiptNotes.Add(goodsReceiptNote);
        await _context.SaveChangesAsync();
        return goodsReceiptNote.Id;
    }

    public async Task<bool> UpdateAsync(GoodsReceiptNote goodsReceiptNote)
    {
        _context.GoodsReceiptNotes.Update(goodsReceiptNote);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.GoodsReceiptNotes.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}
