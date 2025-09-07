using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class InterviewRepository : IInterviewRepository
{
    private readonly ERPDbContext _context;

    public InterviewRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Interview?> GetByIdAsync(Guid id)
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Interview>> GetAllAsync()
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByCandidateAsync(Guid candidateId)
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .Where(i => i.CandidateId == candidateId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByInterviewerAsync(Guid interviewerId)
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .Where(i => i.InterviewerId == interviewerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByStatusAsync(string status)
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .Where(i => i.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Interview interview)
    {
        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync();
        return interview.Id;
    }

    public async Task<bool> UpdateAsync(Interview interview)
    {
        _context.Interviews.Update(interview);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var interview = await _context.Interviews.FindAsync(id);
        if (interview == null) return false;

        _context.Interviews.Remove(interview);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<IEnumerable<Interview>> GetByJobApplicationAsync(Guid jobApplicationId, Guid tenantId)
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .Where(i => i.JobApplicationId == jobApplicationId && i.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        return await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.JobApplication)
            .Where(i => i.InterviewDate >= startDate && i.InterviewDate <= endDate && i.TenantId == tenantId)
            .ToListAsync();
    }
}