using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class JobOpeningRepository : IJobOpeningRepository
{
    private readonly ERPDbContext _context;

    public JobOpeningRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<JobOpening?> GetByIdAsync(Guid id)
    {
        return await _context.JobOpenings
            .Include(j => j.JobApplications)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JobOpening>> GetAllAsync()
    {
        return await _context.JobOpenings
            .Include(j => j.JobApplications)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobOpening>> GetByStatusAsync(string status)
    {
        return await _context.JobOpenings
            .Include(j => j.JobApplications)
            .Where(j => j.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobOpening>> GetByDepartmentAsync(string department)
    {
        return await _context.JobOpenings
            .Include(j => j.JobApplications)
            .Where(j => j.Department == department)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobOpening>> GetByLocationAsync(string location)
    {
        return await _context.JobOpenings
            .Include(j => j.JobApplications)
            .Where(j => j.Location == location)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(JobOpening jobOpening)
    {
        _context.JobOpenings.Add(jobOpening);
        await _context.SaveChangesAsync();
        return jobOpening.Id;
    }

    public async Task<bool> UpdateAsync(JobOpening jobOpening)
    {
        _context.JobOpenings.Update(jobOpening);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var jobOpening = await _context.JobOpenings.FindAsync(id);
        if (jobOpening == null) return false;

        _context.JobOpenings.Remove(jobOpening);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}