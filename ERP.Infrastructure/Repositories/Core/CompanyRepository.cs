using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class CompanyRepository : ICompanyRepository
{
    private readonly ERPDbContext _context;

    public CompanyRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        return await _context.Companies
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _context.Companies
            .Include(c => c.Users)
            .ToListAsync();
    }

    public async Task<IEnumerable<Company>> GetByTenantAsync(Guid tenantId)
    {
        return await _context.Companies
            .Include(c => c.Users)
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Company?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _context.Companies
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);
    }

    public async Task<Guid> CreateAsync(Company company)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company.Id;
    }

    public async Task<bool> UpdateAsync(Company company)
    {
        _context.Companies.Update(company);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return false;

        _context.Companies.Remove(company);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}