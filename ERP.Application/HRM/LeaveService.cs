using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Domain.HRM;

namespace ERP.Application.HRM;

public class LeaveService : ERP.Application.Interfaces.HRM.ILeaveService
{
    private readonly ILeaveRequestRepository _repository;

    public LeaveService(ILeaveRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<LeaveRequestDto> AddLeaveRequestAsync(CreateLeaveRequestDto dto)
    {
        var entity = MapToEntity(dto);
        var createdId = await _repository.CreateAsync(entity);
        var createdEntity = await _repository.GetByIdAsync(createdId);
        return MapToDto(createdEntity!);
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestAsync(UpdateLeaveRequestDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) throw new KeyNotFoundException($"Leave request with ID '{dto.Id}' not found.");

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<LeaveRequestDto> ApproveLeaveAsync(LeaveApprovalDto dto, Guid approverId)
    {
        var entity = await _repository.GetByIdAsync(dto.LeaveRequestId);
        if (entity == null) throw new KeyNotFoundException($"Leave request with ID '{dto.LeaveRequestId}' not found.");

        entity.Status = "Approved";
        entity.ApprovedBy = approverId;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.Notes = dto.Notes;
        
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteLeaveRequestAsync(Guid leaveRequestId)
    {
        await _repository.DeleteAsync(leaveRequestId);
    }

    public async Task<LeaveRequestDto> GetLeaveRequestByIdAsync(Guid leaveRequestId)
    {
        var entity = await _repository.GetByIdAsync(leaveRequestId);
        if (entity == null) throw new KeyNotFoundException($"Leave request with ID '{leaveRequestId}' not found.");
        return MapToDto(entity);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(Guid employeeId)
    {
        var entities = await _repository.GetByEmployeeAsync(employeeId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingLeaveRequestsAsync()
    {
        var entities = await _repository.GetByStatusAsync("Pending");
        return entities.Select(MapToDto);
    }

    public async Task<bool> HasLeaveOverlapAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid? excludeRequestId = null)
    {
        var existingRequests = await _repository.GetByEmployeeAsync(employeeId);
        return existingRequests.Any(r => 
            r.Status == "Approved" && 
            r.Id != excludeRequestId &&
            ((startDate >= r.StartDate && startDate <= r.EndDate) ||
             (endDate >= r.StartDate && endDate <= r.EndDate) ||
             (startDate <= r.StartDate && endDate >= r.EndDate)));
    }

    private static LeaveRequestDto MapToDto(LeaveRequest entity)
    {
        return new LeaveRequestDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            EmployeeId = entity.EmployeeId,
            LeaveTypeId = entity.LeaveTypeId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            DaysRequested = entity.DaysRequested,
            Reason = entity.Reason,
            Status = entity.Status,
            AppliedAt = entity.AppliedAt,
            ApprovedBy = entity.ApprovedBy,
            ApprovedAt = entity.ApprovedAt,
            RejectedBy = entity.RejectedBy,
            RejectedAt = entity.RejectedAt,
            RejectionReason = entity.RejectionReason,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static LeaveRequest MapToEntity(CreateLeaveRequestDto dto)
    {
        return new LeaveRequest
        {
            TenantId = dto.TenantId ?? Guid.Empty,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DaysRequested = dto.DaysRequested,
            TotalDays = (decimal)(dto.EndDate - dto.StartDate).TotalDays + 1,
            Status = "Pending",
            AppliedAt = dto.AppliedAt,
            RequestedAt = DateTime.UtcNow,
            Reason = dto.Reason,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateLeaveRequestDto dto, LeaveRequest entity)
    {
        entity.LeaveTypeId = dto.LeaveTypeId;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.DaysRequested = dto.DaysRequested;
        entity.TotalDays = (decimal)(dto.EndDate - dto.StartDate).TotalDays + 1;
        entity.AppliedAt = dto.AppliedAt;
        entity.Reason = dto.Reason;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}