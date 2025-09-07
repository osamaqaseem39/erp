using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class SalesOrderLineRepository : ISalesOrderLineRepository
{
    private readonly ERPDbContext _context;

    public SalesOrderLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SalesOrderLine?> GetByIdAsync(int id)
    {
        return await _context.SalesOrderLines.FindAsync(id);
    }

    public async Task<IEnumerable<SalesOrderLine>> GetAllAsync()
    {
        return await _context.SalesOrderLines.ToListAsync();
    }

    public async Task<SalesOrderLine> AddAsync(SalesOrderLine entity)
    {
        _context.SalesOrderLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(SalesOrderLine entity)
    {
        _context.SalesOrderLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.SalesOrderLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}