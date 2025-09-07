using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class ActivityRepository : IActivityRepository
{
    private readonly ERPDbContext _context;

    public ActivityRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetAllAsync()
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Activity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity.Id;
    }

    public async Task<bool> UpdateAsync(Activity activity)
    {
        _context.Activities.Update(activity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null) return false;

        _context.Activities.Remove(activity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Activities.AnyAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.LeadId == leadId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByOpportunityAsync(Guid opportunityId)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.OpportunityId == opportunityId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByAssignedToAsync(Guid assignedTo)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.AssignedToUserId == assignedTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByTypeAsync(string activityType)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.ActivityType == activityType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.StartDate >= startDate && a.StartDate <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId)
    {
        return await _context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser)
            .Where(a => a.RelatedEntityType == entityType && a.RelatedEntityId == entityId)
            .ToListAsync();
    }
}