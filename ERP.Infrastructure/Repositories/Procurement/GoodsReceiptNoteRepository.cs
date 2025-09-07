using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class GoodsReceiptNoteRepository : IGoodsReceiptNoteRepository
{
    private readonly ERPDbContext _context;

    public GoodsReceiptNoteRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<GoodsReceiptNote?> GetByIdAsync(Guid id)
    {
        return await _context.GoodsReceiptNotes
            .Include(grn => grn.Vendor)
            .Include(grn => grn.Details)
            .FirstOrDefaultAsync(grn => grn.Id == id);
    }

    public async Task<IEnumerable<GoodsReceiptNote>> GetAllAsync()
    {
        return await _context.GoodsReceiptNotes
            .Include(grn => grn.Vendor)
            .Include(grn => grn.Details)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceiptNote>> GetByVendorAsync(Guid vendorId)
    {
        return await _context.GoodsReceiptNotes
            .Include(grn => grn.Vendor)
            .Include(grn => grn.Details)
            .Where(grn => grn.VendorId == vendorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceiptNote>> GetByStatusAsync(string status)
    {
        return await _context.GoodsReceiptNotes
            .Include(grn => grn.Vendor)
            .Include(grn => grn.Details)
            .Where(grn => grn.Status == status)
            .ToListAsync();
    }

    public async Task<GoodsReceiptNote?> GetByNumberAsync(string grnNumber)
    {
        return await _context.GoodsReceiptNotes
            .Include(grn => grn.Vendor)
            .Include(grn => grn.Details)
            .FirstOrDefaultAsync(grn => grn.GRNNumber == grnNumber);
    }

    public async Task<Guid> CreateAsync(GoodsReceiptNote goodsReceiptNote)
    {
        _context.GoodsReceiptNotes.Add(goodsReceiptNote);
        await _context.SaveChangesAsync();
        return goodsReceiptNote.Id;
    }

    public async Task<bool> UpdateAsync(GoodsReceiptNote goodsReceiptNote)
    {
        _context.GoodsReceiptNotes.Update(goodsReceiptNote);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var goodsReceiptNote = await _context.GoodsReceiptNotes.FindAsync(id);
        if (goodsReceiptNote == null) return false;

        _context.GoodsReceiptNotes.Remove(goodsReceiptNote);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}