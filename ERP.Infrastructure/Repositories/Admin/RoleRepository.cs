using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Admin;

public class RoleRepository : IRoleRepository
{
    private readonly ERPDbContext _context;

    public RoleRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(Guid tenantId)
    {
        return await _context.Roles
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Role> CreateAsync(Role entity)
    {
        if (string.IsNullOrWhiteSpace(entity.RoleName))
            entity.RoleName = entity.Name;

        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Role> UpdateAsync(Role entity)
    {
        if (string.IsNullOrWhiteSpace(entity.RoleName))
            entity.RoleName = entity.Name;

        _context.Roles.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
        if (role == null)
            return false;

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be deleted.");

        _context.Roles.Remove(role);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Role?> GetByNameAsync(string name, Guid tenantId)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.RoleName == name);
    }

    public async Task<IEnumerable<Role>> GetActiveAsync(Guid tenantId)
    {
        return await _context.Roles
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync();
    }

    public async Task<bool> AssignPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId)
    {
        var role = await GetByIdAsync(roleId, tenantId);
        if (role == null)
            return false;

        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in permissionIds.Distinct())
        {
            _context.RolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = roleId,
                PermissionId = permissionId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, Guid tenantId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId && rp.IsActive)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .ToListAsync();
    }
}
