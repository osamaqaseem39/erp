using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Contracts = ERP.Infrastructure.Interfaces.HRM;
using Repos = ERP.Infrastructure.Repositories.HRM;

namespace ERP.Infrastructure.Repositories.Adapters;

/// <summary>
/// Implements the Interfaces.HRM employee contract against EF Core.
/// </summary>
public class EmployeeRepositoryAdapter : Contracts.IEmployeeRepository
{
    private readonly ERPDbContext _db;

    public EmployeeRepositoryAdapter(ERPDbContext db) => _db = db;

    public Task<Employee?> GetByIdAsync(Guid id)
        => _db.Employees.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public async Task<IEnumerable<Employee>> GetAllAsync()
        => await _db.Employees.Where(e => !e.IsDeleted).ToListAsync();

    public async Task<Employee> AddAsync(Employee entity)
    {
        _db.Employees.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Employee entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Employees.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return;
        employee.IsDeleted = true;
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid id)
        => _db.Employees.AnyAsync(e => e.Id == id && !e.IsDeleted);

    public async Task<IEnumerable<Employee>> GetByCompanyIdAsync(Guid companyId)
        => await _db.Employees.Where(e => e.CompanyId == companyId && !e.IsDeleted).ToListAsync();

    public Task<Employee?> GetByCodeAsync(string code)
        => _db.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == code && !e.IsDeleted);
}

/// <summary>
/// Implements the tenant-scoped Interfaces.HRM job opening contract against EF Core.
/// </summary>
public class JobOpeningRepositoryAdapter : Contracts.IJobOpeningRepository
{
    private readonly ERPDbContext _db;

    public JobOpeningRepositoryAdapter(ERPDbContext db) => _db = db;

    private IQueryable<JobOpening> ForTenant(Guid tenantId)
        => _db.JobOpenings.Where(j => tenantId == Guid.Empty || j.TenantId == tenantId);

    public Task<JobOpening?> GetByIdAsync(Guid id, Guid tenantId)
        => ForTenant(tenantId).FirstOrDefaultAsync(j => j.Id == id);

    public async Task<IEnumerable<JobOpening>> GetAllAsync(Guid tenantId)
        => await ForTenant(tenantId).ToListAsync();

    public async Task<JobOpening> CreateAsync(JobOpening entity)
    {
        _db.JobOpenings.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<JobOpening> UpdateAsync(JobOpening entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.JobOpenings.Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.JobOpenings.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // JobOpening stores the department as text, so match on the id's string form.
    public async Task<IEnumerable<JobOpening>> GetByDepartmentAsync(Guid departmentId, Guid tenantId)
    {
        var key = departmentId.ToString();
        return await ForTenant(tenantId).Where(j => j.Department == key).ToListAsync();
    }

    public async Task<IEnumerable<JobOpening>> GetByStatusAsync(string status, Guid tenantId)
        => await ForTenant(tenantId).Where(j => j.Status == status).ToListAsync();

    public async Task<IEnumerable<JobOpening>> GetActiveAsync(Guid tenantId)
        => await ForTenant(tenantId).Where(j => j.Status == "Open").ToListAsync();
}

/// <summary>
/// Implements the tenant-scoped Interfaces.HRM job application contract against EF Core.
/// </summary>
public class JobApplicationRepositoryAdapter : Contracts.IJobApplicationRepository
{
    private readonly ERPDbContext _db;

    public JobApplicationRepositoryAdapter(ERPDbContext db) => _db = db;

    private IQueryable<JobApplication> ForTenant(Guid tenantId)
        => _db.JobApplications.Where(a => tenantId == Guid.Empty || a.TenantId == tenantId);

    public Task<JobApplication?> GetByIdAsync(Guid id, Guid tenantId)
        => ForTenant(tenantId).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<JobApplication>> GetAllAsync(Guid tenantId)
        => await ForTenant(tenantId).ToListAsync();

    public async Task<JobApplication> CreateAsync(JobApplication entity)
    {
        _db.JobApplications.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<JobApplication> UpdateAsync(JobApplication entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.JobApplications.Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.JobApplications.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<JobApplication>> GetByJobOpeningAsync(Guid jobOpeningId, Guid tenantId)
        => await ForTenant(tenantId).Where(a => a.JobOpeningId == jobOpeningId).ToListAsync();

    public async Task<IEnumerable<JobApplication>> GetByCandidateAsync(Guid candidateId, Guid tenantId)
        => await ForTenant(tenantId).Where(a => a.CandidateId == candidateId).ToListAsync();

    public async Task<IEnumerable<JobApplication>> GetByStatusAsync(string status, Guid tenantId)
        => await ForTenant(tenantId).Where(a => a.Status == status).ToListAsync();
}

/// <summary>
/// Implements the tenant-scoped Interfaces.HRM candidate contract against EF Core.
/// </summary>
public class CandidateRepositoryAdapter : Contracts.ICandidateRepository
{
    private readonly ERPDbContext _db;

    public CandidateRepositoryAdapter(ERPDbContext db) => _db = db;

    private IQueryable<Candidate> ForTenant(Guid tenantId)
        => _db.Candidates.Where(c => tenantId == Guid.Empty || c.TenantId == tenantId);

    public Task<Candidate?> GetByIdAsync(Guid id, Guid tenantId)
        => ForTenant(tenantId).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Candidate>> GetAllAsync(Guid tenantId)
        => await ForTenant(tenantId).ToListAsync();

    public async Task<Candidate> CreateAsync(Candidate entity)
    {
        _db.Candidates.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Candidate> UpdateAsync(Candidate entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Candidates.Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.Candidates.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<Candidate?> GetByEmailAsync(string email, Guid tenantId)
        => ForTenant(tenantId).FirstOrDefaultAsync(c => c.Email == email);

    public async Task<IEnumerable<Candidate>> SearchAsync(string searchTerm, Guid tenantId)
        => await ForTenant(tenantId)
            .Where(c => c.FirstName.Contains(searchTerm) || c.LastName.Contains(searchTerm) || c.Email.Contains(searchTerm))
            .ToListAsync();

    public async Task<IEnumerable<Candidate>> GetByStatusAsync(string status, Guid tenantId)
        => await ForTenant(tenantId).Where(c => c.Status == status).ToListAsync();
}

public class InterviewRepositoryAdapter : Contracts.IInterviewRepository
{
    private readonly ERPDbContext _db;

    public InterviewRepositoryAdapter(ERPDbContext db) => _db = db;

    private IQueryable<Interview> ForTenant(Guid tenantId)
        => _db.Interviews.Where(i => tenantId == Guid.Empty || i.TenantId == tenantId);

    public Task<Interview?> GetByIdAsync(Guid id)
        => _db.Interviews.FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Interview>> GetAllAsync()
        => await _db.Interviews.ToListAsync();

    public async Task<Guid> CreateAsync(Interview entity)
    {
        _db.Interviews.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Interview entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Interviews.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Interviews.FindAsync(id);
        if (entity == null) return false;
        _db.Interviews.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Interview>> GetByJobApplicationAsync(Guid jobApplicationId, Guid tenantId)
        => await ForTenant(tenantId).Where(i => i.JobApplicationId == jobApplicationId).ToListAsync();

    public async Task<IEnumerable<Interview>> GetByInterviewerAsync(Guid interviewerId)
        => await _db.Interviews.Where(i => i.InterviewerId == interviewerId).ToListAsync();

    public async Task<IEnumerable<Interview>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
        => await ForTenant(tenantId).Where(i => i.ScheduledDate >= startDate && i.ScheduledDate <= endDate).ToListAsync();

    public async Task<IEnumerable<Interview>> GetByCandidateAsync(Guid candidateId)
        => await _db.Interviews.Where(i => i.CandidateId == candidateId).ToListAsync();
}

public class LeaveRequestRepositoryAdapter : Contracts.ILeaveRequestRepository
{
    private readonly Repos.ILeaveRequestRepository _inner;

    public LeaveRequestRepositoryAdapter(Repos.ILeaveRequestRepository inner) => _inner = inner;

    public Task<LeaveRequest?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<IEnumerable<LeaveRequest>> GetAllAsync() => _inner.GetAllAsync();
    public Task<Guid> CreateAsync(LeaveRequest leaveRequest) => _inner.CreateAsync(leaveRequest);
    public Task<bool> UpdateAsync(LeaveRequest leaveRequest) => _inner.UpdateAsync(leaveRequest);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public async Task<bool> ExistsAsync(Guid id) => await _inner.GetByIdAsync(id) != null;
    public Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId) => _inner.GetByEmployeeAsync(employeeId);
    public Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status) => _inner.GetByStatusAsync(status);
    public Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) => _inner.GetByDateRangeAsync(startDate, endDate);
}

public class PayrollRepositoryAdapter : Contracts.IPayrollRepository
{
    private readonly Repos.IPayrollRepository _inner;

    public PayrollRepositoryAdapter(Repos.IPayrollRepository inner) => _inner = inner;

    public Task<Payroll?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<IEnumerable<Payroll>> GetAllAsync() => _inner.GetAllAsync();
    public Task<Guid> CreateAsync(Payroll payroll) => _inner.CreateAsync(payroll);
    public Task<bool> UpdateAsync(Payroll payroll) => _inner.UpdateAsync(payroll);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public async Task<bool> ExistsAsync(Guid id) => await _inner.GetByIdAsync(id) != null;

    public async Task<IEnumerable<Payroll>> GetByCompanyAsync(Guid companyId)
        => (await _inner.GetAllAsync()).Where(p => p.CompanyId == companyId);

    public Task<IEnumerable<Payroll>> GetByEmployeeAsync(Guid employeeId) => _inner.GetByEmployeeAsync(employeeId);
    public Task<IEnumerable<Payroll>> GetByPeriodAsync(DateTime startDate, DateTime endDate) => _inner.GetByPeriodAsync(startDate, endDate);
}

public class PerformanceReviewRepositoryAdapter : Contracts.IPerformanceReviewRepository
{
    private readonly Repos.IPerformanceReviewRepository _inner;

    public PerformanceReviewRepositoryAdapter(Repos.IPerformanceReviewRepository inner) => _inner = inner;

    public Task<PerformanceReview?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<IEnumerable<PerformanceReview>> GetAllAsync() => _inner.GetAllAsync();
    public Task<Guid> CreateAsync(PerformanceReview performanceReview) => _inner.CreateAsync(performanceReview);
    public Task<bool> UpdateAsync(PerformanceReview performanceReview) => _inner.UpdateAsync(performanceReview);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public async Task<bool> ExistsAsync(Guid id) => await _inner.GetByIdAsync(id) != null;
    public Task<IEnumerable<PerformanceReview>> GetByEmployeeAsync(Guid employeeId) => _inner.GetByEmployeeAsync(employeeId);
    public Task<IEnumerable<PerformanceReview>> GetByReviewerAsync(Guid reviewerId) => _inner.GetByReviewerAsync(reviewerId);

    public async Task<IEnumerable<PerformanceReview>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        => (await _inner.GetAllAsync()).Where(r => r.ReviewPeriodStart <= endDate && r.ReviewPeriodEnd >= startDate);
}

public class TimeEntryRepositoryAdapter : Contracts.ITimeEntryRepository
{
    private readonly Repos.ITimeEntryRepository _inner;

    public TimeEntryRepositoryAdapter(Repos.ITimeEntryRepository inner) => _inner = inner;

    public Task<TimeEntry?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<IEnumerable<TimeEntry>> GetAllAsync() => _inner.GetAllAsync();
    public Task<Guid> CreateAsync(TimeEntry entity) => _inner.CreateAsync(entity);
    public Task<bool> UpdateAsync(TimeEntry entity) => _inner.UpdateAsync(entity);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId) => _inner.GetByEmployeeAsync(employeeId);
    public Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) => _inner.GetByDateRangeAsync(startDate, endDate);

    public async Task<IEnumerable<TimeEntry>> GetByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        => (await _inner.GetByEmployeeAsync(employeeId)).Where(t => t.Date >= startDate && t.Date <= endDate);
}

public class TrainingRepositoryAdapter : Contracts.ITrainingRepository
{
    private readonly Repos.ITrainingRepository _inner;

    public TrainingRepositoryAdapter(Repos.ITrainingRepository inner) => _inner = inner;

    public Task<Training?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<IEnumerable<Training>> GetAllAsync() => _inner.GetAllAsync();
    public Task<Guid> CreateAsync(Training entity) => _inner.CreateAsync(entity);
    public Task<bool> UpdateAsync(Training entity) => _inner.UpdateAsync(entity);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId) => _inner.GetByEmployeeAsync(employeeId);
    public Task<IEnumerable<Training>> GetByStatusAsync(string status) => _inner.GetByStatusAsync(status);
}
