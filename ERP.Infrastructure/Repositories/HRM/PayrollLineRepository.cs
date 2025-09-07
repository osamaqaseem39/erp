using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class PayrollLineRepository : IPayrollLineRepository
{
    private readonly ERPDbContext _context;

    public PayrollLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollLine?> GetByIdAsync(Guid id)
    {
        return await _context.PayrollLines
            .Include(pl => pl.Employee)
            .Include(pl => pl.PayrollComponent)
            .Include(pl => pl.Payroll)
            .FirstOrDefaultAsync(pl => pl.Id == id);
    }

    public async Task<IEnumerable<PayrollLine>> GetAllAsync()
    {
        return await _context.PayrollLines
            .Include(pl => pl.Employee)
            .Include(pl => pl.PayrollComponent)
            .Include(pl => pl.Payroll)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayrollLine>> GetByPayrollAsync(Guid payrollId)
    {
        return await _context.PayrollLines
            .Include(pl => pl.Employee)
            .Include(pl => pl.PayrollComponent)
            .Include(pl => pl.Payroll)
            .Where(pl => pl.PayrollId == payrollId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayrollLine>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.PayrollLines
            .Include(pl => pl.Employee)
            .Include(pl => pl.PayrollComponent)
            .Include(pl => pl.Payroll)
            .Where(pl => pl.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PayrollLine payrollLine)
    {
        _context.PayrollLines.Add(payrollLine);
        await _context.SaveChangesAsync();
        return payrollLine.Id;
    }

    public async Task<bool> UpdateAsync(PayrollLine payrollLine)
    {
        _context.PayrollLines.Update(payrollLine);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payrollLine = await _context.PayrollLines.FindAsync(id);
        if (payrollLine == null) return false;

        _context.PayrollLines.Remove(payrollLine);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}