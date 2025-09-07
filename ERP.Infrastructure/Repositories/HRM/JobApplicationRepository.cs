using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ERPDbContext _context;

    public JobApplicationRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id)
    {
        return await _context.JobApplications
            .Include(ja => ja.Candidate)
            .Include(ja => ja.JobOpening)
            .FirstOrDefaultAsync(ja => ja.Id == id);
    }

    public async Task<IEnumerable<JobApplication>> GetAllAsync()
    {
        return await _context.JobApplications
            .Include(ja => ja.Candidate)
            .Include(ja => ja.JobOpening)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetByCandidateAsync(Guid candidateId)
    {
        return await _context.JobApplications
            .Include(ja => ja.Candidate)
            .Include(ja => ja.JobOpening)
            .Where(ja => ja.CandidateId == candidateId)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetByJobOpeningAsync(Guid jobOpeningId)
    {
        return await _context.JobApplications
            .Include(ja => ja.Candidate)
            .Include(ja => ja.JobOpening)
            .Where(ja => ja.JobOpeningId == jobOpeningId)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetByStatusAsync(string status)
    {
        return await _context.JobApplications
            .Include(ja => ja.Candidate)
            .Include(ja => ja.JobOpening)
            .Where(ja => ja.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(JobApplication jobApplication)
    {
        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync();
        return jobApplication.Id;
    }

    public async Task<bool> UpdateAsync(JobApplication jobApplication)
    {
        _context.JobApplications.Update(jobApplication);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var jobApplication = await _context.JobApplications.FindAsync(id);
        if (jobApplication == null) return false;

        _context.JobApplications.Remove(jobApplication);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}