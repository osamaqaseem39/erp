using ERP.Application.Interfaces.Core;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Core;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly ERPDbContext _context;

    public PermissionService(IPermissionRepository permissionRepository, ERPDbContext context)
    {
        _permissionRepository = permissionRepository;
        _context = context;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync(Guid tenantId, bool activeOnly = true)
    {
        var permissions = await _context.Permissions
            .Where(p => p.TenantId == tenantId && (!activeOnly || p.IsActive))
            .OrderBy(p => p.ModuleName)
            .ThenBy(p => p.ActionName)
            .ToListAsync();

        return permissions.Select(MapToDto);
    }

    public async Task<IEnumerable<PermissionDto>> SeedDefaultPermissionsAsync(Guid tenantId, string createdBy)
    {
        var created = new List<Permission>();

        foreach (var definition in DefaultPermissions.All)
        {
            var exists = await _context.Permissions.AnyAsync(p =>
                p.TenantId == tenantId &&
                p.ModuleName == definition.ModuleName &&
                p.ActionName == definition.ActionName);

            if (exists)
                continue;

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = definition.Name,
                Description = definition.Description,
                Module = definition.Module,
                ModuleName = definition.ModuleName,
                Action = definition.Action,
                ActionName = definition.ActionName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
            };

            await _permissionRepository.CreateAsync(permission);
            created.Add(permission);
        }

        if (created.Count == 0)
        {
            return await GetAllAsync(tenantId);
        }

        return created.Select(MapToDto);
    }

    public async Task AssignAllPermissionsToRoleAsync(Guid roleId, Guid tenantId)
    {
        var permissionIds = await _context.Permissions
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in permissionIds)
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
    }

    private static PermissionDto MapToDto(Permission entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        Name = entity.Name,
        Description = entity.Description,
        Module = entity.Module,
        ModuleName = entity.ModuleName,
        Action = entity.Action,
        ActionName = entity.ActionName,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}
