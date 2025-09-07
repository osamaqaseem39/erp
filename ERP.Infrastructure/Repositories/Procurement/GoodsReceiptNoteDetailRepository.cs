using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class GoodsReceiptNoteDetailRepository : IGoodsReceiptNoteDetailRepository
{
    private readonly ERPDbContext _context;

    public GoodsReceiptNoteDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<GoodsReceiptNoteDetail?> GetByIdAsync(int id)
    {
        return await _context.GoodsReceiptNoteDetails.FindAsync(id);
    }

    public async Task<IEnumerable<GoodsReceiptNoteDetail>> GetAllAsync()
    {
        return await _context.GoodsReceiptNoteDetails.ToListAsync();
    }

    public async Task<GoodsReceiptNoteDetail> AddAsync(GoodsReceiptNoteDetail entity)
    {
        _context.GoodsReceiptNoteDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(GoodsReceiptNoteDetail entity)
    {
        _context.GoodsReceiptNoteDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.GoodsReceiptNoteDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}