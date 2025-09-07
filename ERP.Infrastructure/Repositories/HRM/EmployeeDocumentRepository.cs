using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class EmployeeDocumentRepository : IEmployeeDocumentRepository
{
    private readonly ERPDbContext _context;

    public EmployeeDocumentRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDocument?> GetByIdAsync(Guid id)
    {
        return await _context.EmployeeDocuments
            .Include(ed => ed.Employee)
            .FirstOrDefaultAsync(ed => ed.Id == id);
    }

    public async Task<IEnumerable<EmployeeDocument>> GetAllAsync()
    {
        return await _context.EmployeeDocuments
            .Include(ed => ed.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.EmployeeDocuments
            .Include(ed => ed.Employee)
            .Where(ed => ed.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByTypeAsync(string documentType)
    {
        return await _context.EmployeeDocuments
            .Include(ed => ed.Employee)
            .Where(ed => ed.DocumentType == documentType)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(EmployeeDocument employeeDocument)
    {
        _context.EmployeeDocuments.Add(employeeDocument);
        await _context.SaveChangesAsync();
        return employeeDocument.Id;
    }

    public async Task<bool> UpdateAsync(EmployeeDocument employeeDocument)
    {
        _context.EmployeeDocuments.Update(employeeDocument);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employeeDocument = await _context.EmployeeDocuments.FindAsync(id);
        if (employeeDocument == null) return false;

        _context.EmployeeDocuments.Remove(employeeDocument);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}