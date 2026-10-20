using ERP.Application.Common;
using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class LeaveService : ILeaveService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pending"] = new[] { "Approved", "Rejected", "Cancelled" },
            ["Approved"] = Array.Empty<string>(),
            ["Rejected"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly ILeaveRequestRepository _repository;

    public LeaveService(ILeaveRequestRepository repository)
    {
        _repository = repository;
    }

    public Task<LeaveRequestDto> AddLeaveRequestAsync(CreateLeaveRequestDto dto)
        => AddLeaveRequestAsync(dto, dto.TenantId ?? Guid.Empty, Guid.Empty);

    public async Task<LeaveRequestDto> AddLeaveRequestAsync(CreateLeaveRequestDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));

        var entity = MapToEntity(dto);
        entity.Status = "Pending";
        entity.TenantId = tenantId;
        if (userId != Guid.Empty)
            entity.CreatedBy = userId.ToString();
        var createdId = await _repository.CreateAsync(entity);
        var createdEntity = await _repository.GetByIdAsync(createdId, tenantId)
            ?? throw new InvalidOperationException("Leave request was created but could not be reloaded.");
        return MapToDto(createdEntity);
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestAsync(UpdateLeaveRequestDto dto, Guid tenantId)
    {
        var entity = await RequireScopedAsync(dto.Id, tenantId);
        if (!IsPending(entity.Status))
            throw new InvalidOperationException("Only Pending leave requests can be updated.");

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<LeaveRequestDto> ApproveLeaveAsync(LeaveApprovalDto dto, Guid tenantId, Guid approverId)
    {
        if (approverId == Guid.Empty)
            throw new ArgumentException("Approver is required.", nameof(approverId));

        var entity = await RequireScopedAsync(dto.LeaveRequestId, tenantId);
        if (!DocumentWorkflow.CanTransition(entity.Status, "Approved", Transitions))
            throw new InvalidOperationException($"Only Pending leave requests can be approved (current: {entity.Status}).");
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, approverId))
            throw new InvalidOperationException("Creator cannot approve their own leave request.");

        entity.Status = "Approved";
        entity.ApprovedBy = approverId;
        entity.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = approverId.ToString();

        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<LeaveRequestDto> RejectLeaveAsync(RejectLeaveRequestDto dto, Guid tenantId, Guid rejectedBy)
    {
        if (rejectedBy == Guid.Empty)
            throw new ArgumentException("Rejector is required.", nameof(rejectedBy));

        var entity = await RequireScopedAsync(dto.LeaveRequestId, tenantId);
        if (!DocumentWorkflow.CanTransition(entity.Status, "Rejected", Transitions))
            throw new InvalidOperationException($"Only Pending leave requests can be rejected (current: {entity.Status}).");

        entity.Status = "Rejected";
        entity.RejectedBy = rejectedBy;
        entity.RejectedAt = DateTime.UtcNow;
        entity.RejectionReason = string.IsNullOrWhiteSpace(dto.Reason) ? dto.Comments : dto.Reason;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = rejectedBy.ToString();

        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<LeaveRequestDto> CancelLeaveAsync(Guid leaveRequestId, string? reason, Guid tenantId, Guid userId)
    {
        var entity = await RequireScopedAsync(leaveRequestId, tenantId);
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions))
            throw new InvalidOperationException($"Leave request cannot be cancelled (current: {entity.Status}).");

        entity.Status = "Cancelled";
        entity.Notes = string.IsNullOrWhiteSpace(reason)
            ? entity.Notes
            : (string.IsNullOrWhiteSpace(entity.Notes) ? $"Cancelled: {reason}" : $"{entity.Notes}\nCancelled: {reason}");
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        if (string.IsNullOrWhiteSpace(status))
            return await GetPendingLeaveRequestsAsync(tenantId);
        var entities = await _repository.GetByStatusAsync(status.Trim(), tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> DeleteLeaveRequestAsync(Guid leaveRequestId, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(leaveRequestId, tenantId);
        if (entity == null) return false;
        if (!IsPending(entity.Status)) return false;
        return await _repository.DeleteAsync(leaveRequestId, tenantId);
    }

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid leaveRequestId, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(leaveRequestId, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByEmployeeAsync(employeeId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingLeaveRequestsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync("Pending", tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> HasLeaveOverlapAsync(
        Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId, Guid? excludeRequestId = null)
    {
        if (tenantId == Guid.Empty) return false;
        var existingRequests = await _repository.GetByEmployeeAsync(employeeId, tenantId);
        return existingRequests.Any(r =>
            string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase) &&
            r.Id != excludeRequestId &&
            ((startDate >= r.StartDate && startDate <= r.EndDate) ||
             (endDate >= r.StartDate && endDate <= r.EndDate) ||
             (startDate <= r.StartDate && endDate >= r.EndDate)));
    }

    private async Task<LeaveRequest> RequireScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null)
            throw new KeyNotFoundException($"Leave request with ID '{id}' not found.");
        return entity;
    }

    private static bool IsPending(string? status) =>
        string.Equals(
            string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim(),
            "Pending",
            StringComparison.OrdinalIgnoreCase);

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
