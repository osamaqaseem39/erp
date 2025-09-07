using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces;
using ERP.Infrastructure.Interfaces.Admin;
using ERP.Domain.Core;
using PermissionDto = ERP.Shared.DTOs.Core.PermissionDto;

namespace ERP.Application.Admin;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;

    public RoleService(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        if (isActive.HasValue)
            entities = entities.Where(e => e.IsActive == isActive.Value);
        return entities.Select(MapToDto);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        var createdEntity = await _repository.CreateAsync(entity);
        return MapToDto(createdEntity);
    }

    public async Task<RoleDto> UpdateAsync(UpdateRoleDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) throw new KeyNotFoundException($"Role with ID '{dto.Id}' not found.");

        MapToEntity(dto, entity, userId);
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<RoleDto>> GetActiveAsync(Guid tenantId)
    {
        var entities = await _repository.GetActiveAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> AssignPermissionsAsync(Guid roleId, AssignPermissionsDto dto, Guid tenantId, Guid userId)
    {
        return await _repository.AssignPermissionsAsync(roleId, dto.PermissionIds, tenantId);
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid roleId, Guid tenantId)
    {
        var permissions = await _repository.GetRolePermissionsAsync(roleId, tenantId);
        return permissions.Select(MapToDto);
    }

    private static RoleDto MapToDto(Role entity)
    {
        return new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static Role MapToEntity(CreateRoleDto dto, Guid tenantId, Guid userId)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void MapToEntity(UpdateRoleDto dto, Role entity, Guid userId)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }

    private static PermissionDto MapToDto(Permission entity)
    {
        return new PermissionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Module = entity.Module,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

}