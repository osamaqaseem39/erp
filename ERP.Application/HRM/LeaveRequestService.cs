using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

/// <summary>Thin facade over leave workflow for the alternate leave-request surface.</summary>
public class LeaveRequestService : Interfaces.HRM.ILeaveRequestService
{
    private readonly Interfaces.HRM.ILeaveService _leave;

    public LeaveRequestService(Interfaces.HRM.ILeaveService leave) => _leave = leave;

    public Task<LeaveRequestDto?> GetByIdAsync(Guid id, Guid tenantId)
        => _leave.GetLeaveRequestByIdAsync(id, tenantId);

    public Task<IEnumerable<LeaveRequestDto>> GetAllAsync(Guid tenantId)
        => _leave.GetPendingLeaveRequestsAsync(tenantId);

    public async Task<Guid> CreateAsync(CreateLeaveRequestDto dto)
    {
        var created = await _leave.AddLeaveRequestAsync(dto);
        return created.Id ?? Guid.Empty;
    }

    public async Task<bool> UpdateAsync(UpdateLeaveRequestDto dto, Guid tenantId)
    {
        try
        {
            await _leave.UpdateLeaveRequestAsync(dto, tenantId);
            return true;
        }
        catch (KeyNotFoundException) { return false; }
        catch (InvalidOperationException) { return false; }
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => _leave.DeleteLeaveRequestAsync(id, tenantId);

    public Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
        => _leave.GetLeaveRequestsByEmployeeAsync(employeeId, tenantId);

    public Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(string status, Guid tenantId)
        => _leave.GetLeaveRequestsByStatusAsync(status, tenantId);

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        try
        {
            await _leave.ApproveLeaveAsync(new LeaveApprovalDto { LeaveRequestId = id }, tenantId, userId);
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId, string? reason = null)
    {
        try
        {
            await _leave.RejectLeaveAsync(
                new RejectLeaveRequestDto { LeaveRequestId = id, Reason = reason ?? "Rejected" },
                tenantId,
                userId);
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> CancelAsync(Guid id, LeaveCancellationDto dto, Guid tenantId, Guid userId)
    {
        try
        {
            await _leave.CancelLeaveAsync(id, dto.Reason, tenantId, userId);
            return true;
        }
        catch { return false; }
    }
}
