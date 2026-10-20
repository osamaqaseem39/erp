using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class BenefitRepository : IBenefitRepository
{
    private readonly ERPDbContext _db;

    public BenefitRepository(ERPDbContext db) => _db = db;

    public Task<Benefit?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.Benefits
            .Include(b => b.Enrollments)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId && b.IsActive);

    public async Task<IEnumerable<Benefit>> GetAllAsync(Guid tenantId, string? type = null, bool? isActive = null)
    {
        var q = _db.Benefits.AsNoTracking().Where(b => b.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(b => b.IsActive == isActive.Value);
        else q = q.Where(b => b.IsActive);
        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(b => b.Type == type);
        return await q.OrderBy(b => b.Name).ToListAsync();
    }

    public async Task<IEnumerable<Benefit>> GetActiveAsync(Guid tenantId) =>
        await _db.Benefits.AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();

    public async Task<IEnumerable<Benefit>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        var benefitIds = await _db.EmployeeBenefitEnrollments.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.EmployeeId == employeeId && e.Status == "Active")
            .Select(e => e.BenefitId)
            .ToListAsync();
        return await _db.Benefits.AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.IsActive && benefitIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Benefit entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _db.Benefits.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Benefit entity)
    {
        _db.Benefits.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.Benefits.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        if (entity == null || !entity.IsActive) return false;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _db.SaveChangesAsync() > 0;
    }

    public Task<EmployeeBenefitEnrollment?> GetActiveEnrollmentAsync(Guid benefitId, Guid employeeId, Guid tenantId) =>
        _db.EmployeeBenefitEnrollments.FirstOrDefaultAsync(e =>
            e.TenantId == tenantId && e.BenefitId == benefitId && e.EmployeeId == employeeId && e.Status == "Active");

    public async Task<Guid> CreateEnrollmentAsync(EmployeeBenefitEnrollment enrollment)
    {
        if (enrollment.Id == Guid.Empty) enrollment.Id = Guid.NewGuid();
        _db.EmployeeBenefitEnrollments.Add(enrollment);
        await _db.SaveChangesAsync();
        return enrollment.Id;
    }

    public async Task<bool> UpdateEnrollmentAsync(EmployeeBenefitEnrollment enrollment)
    {
        _db.EmployeeBenefitEnrollments.Update(enrollment);
        return await _db.SaveChangesAsync() > 0;
    }
}
