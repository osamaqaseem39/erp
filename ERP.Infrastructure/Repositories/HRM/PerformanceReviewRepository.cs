using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class PerformanceReviewRepository : IPerformanceReviewRepository
{
    private readonly ERPDbContext _context;

    public PerformanceReviewRepository(ERPDbContext context) => _context = context;

    public Task<PerformanceReview?> GetByIdAsync(Guid id, Guid tenantId) =>
        _context.PerformanceReviews
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.IsActive);

    public async Task<IEnumerable<PerformanceReview>> GetAllAsync(Guid tenantId) =>
        await _context.PerformanceReviews.AsNoTracking()
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderByDescending(p => p.ReviewDate)
            .ToListAsync();

    public async Task<IEnumerable<PerformanceReview>> GetByEmployeeAsync(Guid employeeId, Guid tenantId) =>
        await _context.PerformanceReviews.AsNoTracking()
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && p.IsActive && p.EmployeeId == employeeId)
            .OrderByDescending(p => p.ReviewDate)
            .ToListAsync();

    public async Task<IEnumerable<PerformanceReview>> GetByReviewerAsync(Guid reviewerId, Guid tenantId) =>
        await _context.PerformanceReviews.AsNoTracking()
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && p.IsActive && p.ReviewerId == reviewerId)
            .OrderByDescending(p => p.ReviewDate)
            .ToListAsync();

    public async Task<IEnumerable<PerformanceReview>> GetByPeriodAsync(
        DateTime startDate, DateTime endDate, Guid tenantId) =>
        await _context.PerformanceReviews.AsNoTracking()
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && p.IsActive
                        && p.ReviewPeriodStart <= endDate
                        && p.ReviewPeriodEnd >= startDate)
            .OrderByDescending(p => p.ReviewDate)
            .ToListAsync();

    public async Task<IEnumerable<PerformanceReview>> GetByStatusAsync(string status, Guid tenantId) =>
        await _context.PerformanceReviews.AsNoTracking()
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && p.IsActive && p.Status == status)
            .OrderByDescending(p => p.ReviewDate)
            .ToListAsync();

    public async Task<Guid> CreateAsync(PerformanceReview performanceReview)
    {
        if (performanceReview.Id == Guid.Empty) performanceReview.Id = Guid.NewGuid();
        _context.PerformanceReviews.Add(performanceReview);
        await _context.SaveChangesAsync();
        return performanceReview.Id;
    }

    public async Task<bool> UpdateAsync(PerformanceReview performanceReview)
    {
        _context.PerformanceReviews.Update(performanceReview);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.PerformanceReviews
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        if (entity == null || !entity.IsActive) return false;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
}
