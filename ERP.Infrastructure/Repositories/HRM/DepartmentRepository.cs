using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ERPDbContext _db;

    public DepartmentRepository(ERPDbContext db) => _db = db;

    private IQueryable<Department> ForTenant(Guid tenantId)
        => tenantId == Guid.Empty
            ? _db.Departments.Where(_ => false)
            : _db.Departments.Where(d => d.TenantId == tenantId);

    public async Task<Department?> GetByIdAsync(Guid id, Guid tenantId)
        => await ForTenant(tenantId).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Department>> GetAllAsync(
        Guid tenantId,
        Guid? companyId = null,
        Guid? parentDepartmentId = null)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<Department>();

        var q = ForTenant(tenantId).AsNoTracking();
        if (companyId.HasValue && companyId != Guid.Empty)
            q = q.Where(d => d.CompanyId == companyId);
        if (parentDepartmentId.HasValue)
        {
            q = parentDepartmentId.Value == Guid.Empty
                ? q.Where(d => d.ParentDepartmentId == null)
                : q.Where(d => d.ParentDepartmentId == parentDepartmentId);
        }

        return await q.OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<Department>();

        return await ForTenant(tenantId).AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByManagerAsync(Guid managerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<Department>();

        return await ForTenant(tenantId).AsNoTracking()
            .Where(d => d.ManagerId == managerId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Department department)
    {
        if (department.Id == Guid.Empty) department.Id = Guid.NewGuid();
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();
        return department.Id;
    }

    public async Task<bool> UpdateAsync(Department department)
    {
        _db.Departments.Update(department);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        // Soft-delete to preserve employee FK history
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return await UpdateAsync(entity);
    }

    public async Task<bool> HasEmployeesAsync(Guid departmentId, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        return await _db.Employees.AnyAsync(e =>
            e.DepartmentId == departmentId
            && e.TenantId == tenantId
            && !e.IsDeleted);
    }
}
