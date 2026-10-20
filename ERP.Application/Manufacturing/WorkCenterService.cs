using ERP.Application.Interfaces.Manufacturing;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Manufacturing;

public class WorkCenterService : IWorkCenterService
{
    private readonly IWorkCenterRepository _workCenterRepository;

    public WorkCenterService(IWorkCenterRepository workCenterRepository)
    {
        _workCenterRepository = workCenterRepository;
    }

    public async Task<WorkCenterDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var workCenter = await _workCenterRepository.GetByIdAsync(id, tenantId);
        return workCenter != null ? MapToDto(workCenter) : null;
    }

    public async Task<IEnumerable<WorkCenterDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var workCenters = await _workCenterRepository.GetAllAsync(tenantId);

        if (isActive.HasValue)
            workCenters = workCenters.Where(wc => wc.IsActive == isActive.Value);

        return workCenters.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkCenterDto>> GetActiveAsync(Guid tenantId)
    {
        var workCenters = await _workCenterRepository.GetActiveAsync(tenantId);
        return workCenters.Select(MapToDto);
    }

    public async Task<WorkCenterDto> CreateAsync(CreateWorkCenterDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));

        var code = FirstNonEmpty(dto.WorkCenterCode, dto.Code);
        var name = FirstNonEmpty(dto.WorkCenterName, dto.Name);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Work center code and name are required.");

        var workCenter = new WorkCenter
        {
            Id = Guid.NewGuid(),
            CompanyId = dto.CompanyId,
            WorkCenterCode = code,
            WorkCenterName = name,
            Description = dto.Description,
            WorkCenterType = dto.WorkCenterType ?? string.Empty,
            Capacity = dto.Capacity ?? 0,
            CapacityUnit = dto.CapacityUnit ?? "Hours",
            Notes = dto.Notes,
            IsActive = dto.IsActive ?? true,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _workCenterRepository.CreateAsync(workCenter);
        var created = await _workCenterRepository.GetByIdAsync(createdId, tenantId);
        return MapToDto(created!);
    }

    public async Task<WorkCenterDto> UpdateAsync(UpdateWorkCenterDto dto, Guid tenantId, Guid userId)
    {
        var workCenter = await _workCenterRepository.GetByIdAsync(dto.Id, tenantId);
        if (workCenter == null)
            throw new KeyNotFoundException($"Work Center with ID '{dto.Id}' not found.");

        var code = FirstNonEmpty(dto.WorkCenterCode, dto.Code);
        var name = FirstNonEmpty(dto.WorkCenterName, dto.Name);
        if (code != null) workCenter.WorkCenterCode = code;
        if (name != null) workCenter.WorkCenterName = name;
        if (dto.Description != null) workCenter.Description = dto.Description;
        if (dto.WorkCenterType != null) workCenter.WorkCenterType = dto.WorkCenterType;
        if (dto.Capacity.HasValue) workCenter.Capacity = dto.Capacity.Value;
        if (dto.CapacityUnit != null) workCenter.CapacityUnit = dto.CapacityUnit;
        if (dto.Notes != null) workCenter.Notes = dto.Notes;
        if (dto.IsActive.HasValue) workCenter.IsActive = dto.IsActive.Value;
        workCenter.UpdatedBy = userId.ToString();
        workCenter.UpdatedAt = DateTime.UtcNow;

        await _workCenterRepository.UpdateAsync(workCenter);
        return MapToDto(workCenter);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => await _workCenterRepository.DeleteAsync(id, tenantId);

    private static WorkCenterDto MapToDto(WorkCenter workCenter)
    {
        return new WorkCenterDto
        {
            Id = workCenter.Id,
            WorkCenterCode = workCenter.WorkCenterCode,
            Code = workCenter.WorkCenterCode,
            WorkCenterName = workCenter.WorkCenterName,
            Name = workCenter.WorkCenterName,
            Description = workCenter.Description,
            WorkCenterType = workCenter.WorkCenterType,
            Capacity = workCenter.Capacity,
            CapacityUnit = workCenter.CapacityUnit,
            Notes = workCenter.Notes,
            IsActive = workCenter.IsActive,
            TenantId = workCenter.TenantId,
            CreatedBy = workCenter.CreatedBy,
            CreatedAt = workCenter.CreatedAt,
            UpdatedBy = workCenter.UpdatedBy,
            UpdatedAt = workCenter.UpdatedAt
        };
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();
}
