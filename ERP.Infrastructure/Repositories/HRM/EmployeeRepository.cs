using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Shared;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ERPDbContext _context;

    public EmployeeRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee.Id;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(Guid employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee != null)
        {
            employee.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid employeeId)
    {
        return await _context.Employees
            .Include(e => e.Manager)
            .Include(e => e.EmploymentContract)
            .Include(e => e.Addresses)
            .Include(e => e.Documents)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);
    }

    public async Task<PagedResult<Employee>> GetAllEmployeesAsync(EmployeeQuery filter)
    {
        var query = _context.Employees
            .Include(e => e.EmploymentContract)
            .Where(e => !e.IsDeleted);

        if (filter.TenantId.HasValue)
            query = query.Where(e => e.TenantId == filter.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(e => e.CompanyId == filter.CompanyId);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(e => e.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(e => 
                e.FirstName.ToLower().Contains(searchTerm) ||
                e.LastName.ToLower().Contains(searchTerm) ||
                e.EmployeeCode.ToLower().Contains(searchTerm) ||
                (e.WorkEmail != null && e.WorkEmail.ToLower().Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();

        var employees = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Employee>
        {
            Items = employees,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
    }

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid? excludeEmployeeId = null)
    {
        var query = _context.Employees.Where(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
        
        if (excludeEmployeeId.HasValue)
            query = query.Where(e => e.Id != excludeEmployeeId.Value);

        return !await query.AnyAsync();
    }
}