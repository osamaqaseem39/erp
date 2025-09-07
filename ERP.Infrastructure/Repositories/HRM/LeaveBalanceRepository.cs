using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly ERPDbContext _context;

    public LeaveBalanceRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveBalance?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveBalances
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<LeaveBalance>> GetAllAsync()
    {
        return await _context.LeaveBalances
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.LeaveBalances
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveBalance>> GetByLeaveTypeAsync(Guid leaveTypeId)
    {
        return await _context.LeaveBalances
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.LeaveTypeId == leaveTypeId)
            .ToListAsync();
    }

    public async Task<LeaveBalance?> GetByEmployeeAndLeaveTypeAsync(Guid employeeId, Guid leaveTypeId)
    {
        return await _context.LeaveBalances
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.EmployeeId == employeeId && l.LeaveTypeId == leaveTypeId);
    }

    public async Task<Guid> CreateAsync(LeaveBalance leaveBalance)
    {
        _context.LeaveBalances.Add(leaveBalance);
        await _context.SaveChangesAsync();
        return leaveBalance.Id;
    }

    public async Task<bool> UpdateAsync(LeaveBalance leaveBalance)
    {
        _context.LeaveBalances.Update(leaveBalance);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var leaveBalance = await _context.LeaveBalances.FindAsync(id);
        if (leaveBalance == null) return false;

        _context.LeaveBalances.Remove(leaveBalance);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}