using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class EmploymentContractRepository : IEmploymentContractRepository
{
    private readonly ERPDbContext _context;

    public EmploymentContractRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<EmploymentContract?> GetByIdAsync(Guid id)
    {
        return await _context.EmploymentContracts
            .Include(ec => ec.Employee)
            .FirstOrDefaultAsync(ec => ec.Id == id);
    }

    public async Task<IEnumerable<EmploymentContract>> GetAllAsync()
    {
        return await _context.EmploymentContracts
            .Include(ec => ec.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmploymentContract>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.EmploymentContracts
            .Include(ec => ec.Employee)
            .Where(ec => ec.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmploymentContract>> GetByStatusAsync(string status)
    {
        return await _context.EmploymentContracts
            .Include(ec => ec.Employee)
            .Where(ec => ec.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(EmploymentContract employmentContract)
    {
        _context.EmploymentContracts.Add(employmentContract);
        await _context.SaveChangesAsync();
        return employmentContract.Id;
    }

    public async Task<bool> UpdateAsync(EmploymentContract employmentContract)
    {
        _context.EmploymentContracts.Update(employmentContract);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employmentContract = await _context.EmploymentContracts.FindAsync(id);
        if (employmentContract == null) return false;

        _context.EmploymentContracts.Remove(employmentContract);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}