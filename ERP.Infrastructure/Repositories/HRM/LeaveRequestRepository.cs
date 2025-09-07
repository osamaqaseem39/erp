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

    public async Task<LeaveRequest?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(Guid leaveTypeId)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.LeaveTypeId == leaveTypeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.StartDate >= startDate && l.EndDate <= endDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(LeaveRequest leaveRequest)
    {
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest.Id;
    }

    public async Task<bool> UpdateAsync(LeaveRequest leaveRequest)
    {
        _context.LeaveRequests.Update(leaveRequest);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null) return false;

        _context.LeaveRequests.Remove(leaveRequest);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}