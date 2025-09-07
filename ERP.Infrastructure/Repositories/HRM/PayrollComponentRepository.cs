using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class PayrollComponentRepository : IPayrollComponentRepository
{
    private readonly ERPDbContext _context;

    public PayrollComponentRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollComponent?> GetByIdAsync(Guid id)
    {
        return await _context.PayrollComponents.FindAsync(id);
    }

    public async Task<IEnumerable<PayrollComponent>> GetAllAsync()
    {
        return await _context.PayrollComponents.ToListAsync();
    }

    public async Task<IEnumerable<PayrollComponent>> GetActiveAsync()
    {
        return await _context.PayrollComponents
            .Where(pc => pc.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayrollComponent>> GetByTypeAsync(string type)
    {
        return await _context.PayrollComponents
            .Where(pc => pc.Type == type)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PayrollComponent payrollComponent)
    {
        _context.PayrollComponents.Add(payrollComponent);
        await _context.SaveChangesAsync();
        return payrollComponent.Id;
    }

    public async Task<bool> UpdateAsync(PayrollComponent payrollComponent)
    {
        _context.PayrollComponents.Update(payrollComponent);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payrollComponent = await _context.PayrollComponents.FindAsync(id);
        if (payrollComponent == null) return false;

        _context.PayrollComponents.Remove(payrollComponent);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}