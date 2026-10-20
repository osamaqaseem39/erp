using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly ERPDbContext _context;

    public LeaveRequestRepository(ERPDbContext context)
    {
        _context = context;
    }

    // Avoid Include on required Employee/LeaveType — missing rows become INNER JOINs and hide requests.
    private IQueryable<LeaveRequest> Query(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return _context.LeaveRequests.Where(_ => false);
        return _context.LeaveRequests.Where(l => l.TenantId == tenantId);
    }

    private async Task LoadRelatedAsync(LeaveRequest? entity)
    {
        if (entity == null) return;
        await _context.Entry(entity).Reference(l => l.Employee).LoadAsync();
        await _context.Entry(entity).Reference(l => l.LeaveType).LoadAsync();
    }

    private async Task LoadRelatedAsync(IEnumerable<LeaveRequest> entities)
    {
        foreach (var entity in entities)
        {
            await _context.Entry(entity).Reference(l => l.Employee).LoadAsync();
            await _context.Entry(entity).Reference(l => l.LeaveType).LoadAsync();
        }
    }

    public async Task<LeaveRequest?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await Query(tenantId).FirstOrDefaultAsync(l => l.Id == id);
        await LoadRelatedAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync(Guid tenantId)
    {
        var list = await Query(tenantId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(l => l.EmployeeId == employeeId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status, Guid tenantId)
    {
        var list = await Query(tenantId).Where(l => l.Status == status).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(Guid leaveTypeId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(l => l.LeaveTypeId == leaveTypeId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        var list = await Query(tenantId)
            .Where(l => l.StartDate >= startDate && l.EndDate <= endDate)
            .ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(LeaveRequest leaveRequest)
    {
        if (leaveRequest.Id == Guid.Empty) leaveRequest.Id = Guid.NewGuid();
        if (leaveRequest.CreatedAt == default) leaveRequest.CreatedAt = DateTime.UtcNow;
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest.Id;
    }

    public async Task<bool> UpdateAsync(LeaveRequest leaveRequest)
    {
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        _context.LeaveRequests.Update(leaveRequest);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var leaveRequest = await GetByIdAsync(id, tenantId);
        if (leaveRequest == null) return false;
        _context.LeaveRequests.Remove(leaveRequest);
        return await _context.SaveChangesAsync() > 0;
    }
}
