using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemCategoryRepository : IItemCategoryRepository
{
    private readonly ERPDbContext _context;

    public ItemCategoryRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ItemCategory?> GetByIdAsync(int id)
    {
        return await _context.ItemCategories.FindAsync(id);
    }

    public async Task<IEnumerable<ItemCategory>> GetAllAsync()
    {
        return await _context.ItemCategories.ToListAsync();
    }

    public async Task<ItemCategory> AddAsync(ItemCategory entity)
    {
        _context.ItemCategories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ItemCategory entity)
    {
        _context.ItemCategories.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.ItemCategories.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}