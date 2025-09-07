using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemVariantAttributeRepository : IItemVariantAttributeRepository
{
    private readonly ERPDbContext _context;

    public ItemVariantAttributeRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ItemVariantAttribute?> GetByIdAsync(int id)
    {
        return await _context.ItemVariantAttributes.FindAsync(id);
    }

    public async Task<IEnumerable<ItemVariantAttribute>> GetAllAsync()
    {
        return await _context.ItemVariantAttributes.ToListAsync();
    }

    public async Task<ItemVariantAttribute> AddAsync(ItemVariantAttribute entity)
    {
        _context.ItemVariantAttributes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ItemVariantAttribute entity)
    {
        _context.ItemVariantAttributes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.ItemVariantAttributes.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}