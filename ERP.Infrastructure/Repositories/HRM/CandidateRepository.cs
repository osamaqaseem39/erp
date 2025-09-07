using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class CandidateRepository : ICandidateRepository
{
    private readonly ERPDbContext _context;

    public CandidateRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Candidate?> GetByIdAsync(Guid id)
    {
        return await _context.Candidates
            .Include(c => c.JobApplications)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Candidate>> GetAllAsync()
    {
        return await _context.Candidates
            .Include(c => c.JobApplications)
            .ToListAsync();
    }

    public async Task<IEnumerable<Candidate>> GetByStatusAsync(string status)
    {
        return await _context.Candidates
            .Include(c => c.JobApplications)
            .Where(c => c.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Candidate>> GetByJobOpeningAsync(Guid jobOpeningId)
    {
        return await _context.Candidates
            .Include(c => c.JobApplications)
            .Where(c => c.JobApplications.Any(ja => ja.JobOpeningId == jobOpeningId))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Candidate candidate)
    {
        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();
        return candidate.Id;
    }

    public async Task<bool> UpdateAsync(Candidate candidate)
    {
        _context.Candidates.Update(candidate);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var candidate = await _context.Candidates.FindAsync(id);
        if (candidate == null) return false;

        _context.Candidates.Remove(candidate);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}