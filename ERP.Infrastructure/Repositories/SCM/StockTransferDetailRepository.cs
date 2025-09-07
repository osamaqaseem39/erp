using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class StockTransferDetailRepository : IStockTransferDetailRepository
{
    private readonly ERPDbContext _context;

    public StockTransferDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<StockTransferDetail?> GetByIdAsync(int id)
    {
        return await _context.StockTransferDetails.FindAsync(id);
    }

    public async Task<IEnumerable<StockTransferDetail>> GetAllAsync()
    {
        return await _context.StockTransferDetails.ToListAsync();
    }

    public async Task<StockTransferDetail> AddAsync(StockTransferDetail entity)
    {
        _context.StockTransferDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(StockTransferDetail entity)
    {
        _context.StockTransferDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.StockTransferDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}