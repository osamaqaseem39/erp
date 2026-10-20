using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class TrainingRepository : ITrainingRepository
{
    private readonly ERPDbContext _context;

    public TrainingRepository(ERPDbContext context) => _context = context;

    public Task<Training?> GetByIdAsync(Guid id, Guid tenantId) =>
        _context.Trainings
            .Include(t => t.Enrollments)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.IsActive);

    public async Task<IEnumerable<Training>> GetAllAsync(Guid tenantId) =>
        await _context.Trainings.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.IsActive)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

    public async Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        var trainingIds = await _context.TrainingEnrollments.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.EmployeeId == employeeId
                        && e.Status != "Dropped")
            .Select(e => e.TrainingId)
            .Distinct()
            .ToListAsync();

        return await _context.Trainings.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.IsActive && trainingIds.Contains(t.Id))
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Training>> GetByStatusAsync(string status, Guid tenantId) =>
        await _context.Trainings.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.IsActive && t.Status == status)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

    public async Task<Guid> CreateAsync(Training training)
    {
        if (training.Id == Guid.Empty) training.Id = Guid.NewGuid();
        _context.Trainings.Add(training);
        await _context.SaveChangesAsync();
        return training.Id;
    }

    public async Task<bool> UpdateAsync(Training training)
    {
        _context.Trainings.Update(training);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var training = await _context.Trainings
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        if (training == null || !training.IsActive) return false;
        training.IsActive = false;
        training.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public Task<TrainingEnrollment?> GetEnrollmentAsync(Guid trainingId, Guid employeeId, Guid tenantId) =>
        _context.TrainingEnrollments.FirstOrDefaultAsync(e =>
            e.TrainingId == trainingId && e.EmployeeId == employeeId && e.TenantId == tenantId
            && e.Status != "Dropped");

    public async Task<Guid> CreateEnrollmentAsync(TrainingEnrollment enrollment)
    {
        if (enrollment.Id == Guid.Empty) enrollment.Id = Guid.NewGuid();
        _context.TrainingEnrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment.Id;
    }

    public async Task<bool> UpdateEnrollmentAsync(TrainingEnrollment enrollment)
    {
        _context.TrainingEnrollments.Update(enrollment);
        return await _context.SaveChangesAsync() > 0;
    }

    public Task<int> CountActiveEnrollmentsAsync(Guid trainingId, Guid tenantId) =>
        _context.TrainingEnrollments.CountAsync(e =>
            e.TrainingId == trainingId && e.TenantId == tenantId
            && (e.Status == "Enrolled" || e.Status == "Completed"));
}
