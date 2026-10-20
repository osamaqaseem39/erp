using ERP.Application.Interfaces.HRM;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.HRM;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly ERPDbContext _db;

    public LeaveBalanceService(ERPDbContext db) => _db = db;

    public async Task<IEnumerable<LeaveBalanceDto>> GetAllAsync(
        Guid tenantId, Guid? employeeId = null, Guid? leaveTypeId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<LeaveBalanceDto>();

        var q = _db.LeaveBalances.AsNoTracking()
            .Include(b => b.Employee)
            .Include(b => b.LeaveType)
            .Where(b => b.TenantId == tenantId);
        if (employeeId.HasValue && employeeId != Guid.Empty)
            q = q.Where(b => b.EmployeeId == employeeId);
        if (leaveTypeId.HasValue && leaveTypeId != Guid.Empty)
            q = q.Where(b => b.LeaveTypeId == leaveTypeId);

        var rows = await q.OrderBy(b => b.Employee.LastName).ThenBy(b => b.LeaveType.Name).ToListAsync();
        return rows.Select(b => new LeaveBalanceDto
        {
            Id = b.Id,
            EmployeeId = b.EmployeeId,
            EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}".Trim(),
            LeaveTypeId = b.LeaveTypeId,
            LeaveTypeName = b.LeaveType.Name,
            Accrued = b.Accrued,
            Taken = b.Taken,
            Available = b.Available,
            CreatedAt = b.CreatedAt
        });
    }
}
