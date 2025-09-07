using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class PerformanceReviewRepository : IPerformanceReviewRepository
{
    private readonly ERPDbContext _context;

    public PerformanceReviewRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PerformanceReview?> GetByIdAsync(Guid id)
    {
        return await _context.PerformanceReviews
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PerformanceReview>> GetAllAsync()
    {
        return await _context.PerformanceReviews
            .Include(p => p.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.PerformanceReviews
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetByReviewerAsync(Guid reviewerId)
    {
        return await _context.PerformanceReviews
            .Include(p => p.Employee)
            .Where(p => p.ReviewerId == reviewerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetByStatusAsync(string status)
    {
        return await _context.PerformanceReviews
            .Include(p => p.Employee)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PerformanceReview performanceReview)
    {
        _context.PerformanceReviews.Add(performanceReview);
        await _context.SaveChangesAsync();
        return performanceReview.Id;
    }

    public async Task<bool> UpdateAsync(PerformanceReview performanceReview)
    {
        _context.PerformanceReviews.Update(performanceReview);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var performanceReview = await _context.PerformanceReviews.FindAsync(id);
        if (performanceReview == null) return false;

        _context.PerformanceReviews.Remove(performanceReview);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}