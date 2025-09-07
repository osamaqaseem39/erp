using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemRepository : IItemRepository
{
    private readonly ERPDbContext _context;

    public ItemRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items.FindAsync(id);
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _context.Items.ToListAsync();
    }

    public async Task<Item> AddAsync(Item entity)
    {
        _context.Items.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Item entity)
    {
        _context.Items.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Items.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}