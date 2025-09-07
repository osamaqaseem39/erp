using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class EmployeeAddressRepository : IEmployeeAddressRepository
{
    private readonly ERPDbContext _context;

    public EmployeeAddressRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeAddress?> GetByIdAsync(Guid id)
    {
        return await _context.EmployeeAddresses
            .Include(ea => ea.Employee)
            .FirstOrDefaultAsync(ea => ea.Id == id);
    }

    public async Task<IEnumerable<EmployeeAddress>> GetAllAsync()
    {
        return await _context.EmployeeAddresses
            .Include(ea => ea.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeAddress>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.EmployeeAddresses
            .Include(ea => ea.Employee)
            .Where(ea => ea.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeAddress>> GetByTypeAsync(string addressType)
    {
        return await _context.EmployeeAddresses
            .Include(ea => ea.Employee)
            .Where(ea => ea.AddressType == addressType)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(EmployeeAddress employeeAddress)
    {
        _context.EmployeeAddresses.Add(employeeAddress);
        await _context.SaveChangesAsync();
        return employeeAddress.Id;
    }

    public async Task<bool> UpdateAsync(EmployeeAddress employeeAddress)
    {
        _context.EmployeeAddresses.Update(employeeAddress);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employeeAddress = await _context.EmployeeAddresses.FindAsync(id);
        if (employeeAddress == null) return false;

        _context.EmployeeAddresses.Remove(employeeAddress);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}