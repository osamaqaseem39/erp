using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class RoleRepository : IRoleRepository
{
    private readonly ERPDbContext _context;

    public RoleRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetActiveAsync()
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Guid> CreateAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role.Id;
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        _context.Roles.Remove(role);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}