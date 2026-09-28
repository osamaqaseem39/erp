using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class ActivityRepository : CrmRepositoryBase, IActivityRepository
{
    public ActivityRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Activity> ActivitiesQuery() =>
        ApplyTenant(Context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Include(a => a.AssignedToUser));

    public async Task<Activity?> GetByIdAsync(Guid id) =>
        await ActivitiesQuery().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Activity>> GetAllAsync() =>
        await ActivitiesQuery().ToListAsync();

    public async Task<Guid> CreateAsync(Activity activity)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            activity.TenantId = TenantId.Value;

        Context.Activities.Add(activity);
        await Context.SaveChangesAsync();
        return activity.Id;
    }

    public async Task<bool> UpdateAsync(Activity activity)
    {
        if (!MatchesTenant(activity)) return false;
        Context.Activities.Update(activity);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var activity = await GetByIdAsync(id);
        if (activity == null) return false;
        Context.Activities.Remove(activity);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid id) =>
        await ActivitiesQuery().AnyAsync(a => a.Id == id);

    public async Task<IEnumerable<Activity>> GetByCustomerAsync(Guid customerId) =>
        await ActivitiesQuery().Where(a => a.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId) =>
        await ActivitiesQuery().Where(a => a.LeadId == leadId).ToListAsync();

    public async Task<IEnumerable<Activity>> GetByOpportunityAsync(Guid opportunityId) =>
        await ActivitiesQuery().Where(a => a.OpportunityId == opportunityId).ToListAsync();

    public async Task<IEnumerable<Activity>> GetByAssignedToAsync(Guid assignedTo) =>
        await ActivitiesQuery().Where(a => a.AssignedToUserId == assignedTo).ToListAsync();

    public async Task<IEnumerable<Activity>> GetByTypeAsync(string activityType) =>
        await ActivitiesQuery().Where(a => a.ActivityType == activityType).ToListAsync();

    public async Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        await ActivitiesQuery().Where(a => a.StartDate >= startDate && a.StartDate <= endDate).ToListAsync();

    public async Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId) =>
        await ActivitiesQuery().Where(a => a.RelatedEntityType == entityType && a.RelatedEntityId == entityId).ToListAsync();
}
