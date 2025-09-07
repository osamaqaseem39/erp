using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class PayrollRepository : IPayrollRepository
{
    private readonly ERPDbContext _context;

    public PayrollRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Payroll?> GetByIdAsync(Guid id)
    {
        return await _context.Payrolls
            .Include(p => p.PayrollLines)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payroll>> GetAllAsync()
    {
        return await _context.Payrolls
            .Include(p => p.PayrollLines)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.Payrolls
            .Include(p => p.PayrollLines)
            .Where(p => p.PayrollLines.Any(pl => pl.EmployeeId == employeeId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Payrolls
            .Include(p => p.PayrollLines)
            .Where(p => p.PayPeriodStart >= startDate && p.PayPeriodEnd <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetByStatusAsync(string status)
    {
        return await _context.Payrolls
            .Include(p => p.PayrollLines)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Payroll payroll)
    {
        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();
        return payroll.Id;
    }

    public async Task<bool> UpdateAsync(Payroll payroll)
    {
        _context.Payrolls.Update(payroll);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payroll = await _context.Payrolls.FindAsync(id);
        if (payroll == null) return false;

        _context.Payrolls.Remove(payroll);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}