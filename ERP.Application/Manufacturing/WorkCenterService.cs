using ERP.Application.Interfaces.Manufacturing;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Domain.Manufacturing;

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
        var workCenter = await _workCenterRepository.GetByIdAsync(id);
        return workCenter != null ? MapToDto(workCenter) : null;
    }

    public async Task<IEnumerable<WorkCenterDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var workCenters = await _workCenterRepository.GetAllAsync();
        
        if (isActive.HasValue)
            workCenters = workCenters.Where(wc => wc.IsActive == isActive.Value);

        return workCenters.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkCenterDto>> GetActiveAsync(Guid tenantId)
    {
        var workCenters = await _workCenterRepository.GetActiveAsync();
        return workCenters.Select(MapToDto);
    }

    public async Task<WorkCenterDto> CreateAsync(CreateWorkCenterDto dto, Guid tenantId, Guid userId)
    {
        var workCenter = new WorkCenter
        {
            Id = Guid.NewGuid(),
            WorkCenterCode = dto.Code,
            WorkCenterName = dto.Name,
            Description = dto.Description,
            Capacity = dto.Capacity ?? 0,
            IsActive = dto.IsActive ?? true,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _workCenterRepository.CreateAsync(workCenter);
        var created = await _workCenterRepository.GetByIdAsync(createdId);
        return MapToDto(created!);
    }

    public async Task<WorkCenterDto> UpdateAsync(UpdateWorkCenterDto dto, Guid tenantId, Guid userId)
    {
        var workCenter = await _workCenterRepository.GetByIdAsync(dto.Id);
        if (workCenter == null)
            throw new KeyNotFoundException($"Work Center with ID '{dto.Id}' not found.");

        workCenter.WorkCenterCode = dto.Code;
        workCenter.WorkCenterName = dto.Name;
        workCenter.Description = dto.Description;
        workCenter.Capacity = dto.Capacity ?? workCenter.Capacity;
        workCenter.IsActive = dto.IsActive ?? workCenter.IsActive;
        workCenter.UpdatedBy = userId.ToString();
        workCenter.UpdatedAt = DateTime.UtcNow;

        await _workCenterRepository.UpdateAsync(workCenter);
        return MapToDto(workCenter);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _workCenterRepository.DeleteAsync(id);
    }

    private static WorkCenterDto MapToDto(WorkCenter workCenter)
    {
        return new WorkCenterDto
        {
            Id = workCenter.Id,
            Code = workCenter.WorkCenterCode,
            Name = workCenter.WorkCenterName,
            Description = workCenter.Description,
            Capacity = workCenter.Capacity,
            IsActive = workCenter.IsActive,
            TenantId = workCenter.TenantId,
            CreatedBy = workCenter.CreatedBy,
            CreatedAt = workCenter.CreatedAt,
            UpdatedBy = workCenter.UpdatedBy,
            UpdatedAt = workCenter.UpdatedAt
        };
    }
}