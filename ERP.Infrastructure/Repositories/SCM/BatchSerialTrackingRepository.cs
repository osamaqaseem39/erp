using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class BatchSerialTrackingRepository : IBatchSerialTrackingRepository
{
    private readonly ERPDbContext _context;

    public BatchSerialTrackingRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<BatchSerialTracking?> GetByIdAsync(int id)
    {
        return await _context.BatchSerialTrackings.FindAsync(id);
    }

    public async Task<IEnumerable<BatchSerialTracking>> GetAllAsync()
    {
        return await _context.BatchSerialTrackings.ToListAsync();
    }

    public async Task<BatchSerialTracking> AddAsync(BatchSerialTracking entity)
    {
        _context.BatchSerialTrackings.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(BatchSerialTracking entity)
    {
        _context.BatchSerialTrackings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.BatchSerialTrackings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}