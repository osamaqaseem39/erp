using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository) => _repository = repository;

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(
        Guid tenantId,
        Guid? companyId = null,
        Guid? parentDepartmentId = null)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<DepartmentDto>();

        return (await _repository.GetAllAsync(tenantId, companyId, parentDepartmentId)).Select(Map);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Department name is required.");

        if (dto.ParentDepartmentId.HasValue && dto.ParentDepartmentId != Guid.Empty)
        {
            var parent = await _repository.GetByIdAsync(dto.ParentDepartmentId.Value, tenantId);
            if (parent == null) throw new ArgumentException("Parent department not found.");
        }

        var entity = new Department
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            ManagerId = dto.ManagerId == Guid.Empty ? null : dto.ManagerId,
            ParentDepartmentId = dto.ParentDepartmentId == Guid.Empty ? null : dto.ParentDepartmentId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString()
        };

        await _repository.CreateAsync(entity);
        return Map(entity);
    }

    public async Task<DepartmentDto> UpdateAsync(UpdateDepartmentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var entity = await _repository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Department '{dto.Id}' not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Department name is required.");

        if (dto.ParentDepartmentId.HasValue
            && dto.ParentDepartmentId != Guid.Empty
            && dto.ParentDepartmentId == dto.Id)
            throw new ArgumentException("Department cannot be its own parent.");

        if (dto.ParentDepartmentId.HasValue && dto.ParentDepartmentId != Guid.Empty)
        {
            var parent = await _repository.GetByIdAsync(dto.ParentDepartmentId.Value, tenantId);
            if (parent == null) throw new ArgumentException("Parent department not found.");
        }

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.ManagerId = dto.ManagerId == Guid.Empty ? null : dto.ManagerId;
        entity.ParentDepartmentId = dto.ParentDepartmentId == Guid.Empty ? null : dto.ParentDepartmentId;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();

        await _repository.UpdateAsync(entity);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<DepartmentDto>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<DepartmentDto>();

        return (await _repository.GetActiveAsync(tenantId)).Select(Map);
    }

    public async Task<IEnumerable<DepartmentDto>> GetByManagerAsync(Guid managerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<DepartmentDto>();

        return (await _repository.GetByManagerAsync(managerId, tenantId)).Select(Map);
    }

    private static DepartmentDto Map(Department entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        Name = entity.Name,
        Description = entity.Description,
        ManagerId = entity.ManagerId,
        ParentDepartmentId = entity.ParentDepartmentId,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy
    };
}
