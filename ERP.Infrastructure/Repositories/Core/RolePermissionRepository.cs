using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ERPDbContext _context;

    public RolePermissionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<RolePermission?> GetByIdAsync(int id)
    {
        return await _context.RolePermissions.FindAsync(id);
    }

    public async Task<IEnumerable<RolePermission>> GetAllAsync()
    {
        return await _context.RolePermissions.ToListAsync();
    }

    public async Task<RolePermission> AddAsync(RolePermission entity)
    {
        _context.RolePermissions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(RolePermission entity)
    {
        _context.RolePermissions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.RolePermissions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}