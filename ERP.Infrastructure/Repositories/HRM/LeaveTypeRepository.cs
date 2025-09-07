using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly ERPDbContext _context;

    public LeaveTypeRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveType?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveTypes
            .Include(l => l.LeaveRequests)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<LeaveType>> GetAllAsync()
    {
        return await _context.LeaveTypes
            .Include(l => l.LeaveRequests)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveType>> GetActiveAsync()
    {
        return await _context.LeaveTypes
            .Include(l => l.LeaveRequests)
            .Where(l => l.IsActive)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(LeaveType leaveType)
    {
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();
        return leaveType.Id;
    }

    public async Task<bool> UpdateAsync(LeaveType leaveType)
    {
        _context.LeaveTypes.Update(leaveType);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);
        if (leaveType == null) return false;

        _context.LeaveTypes.Remove(leaveType);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}