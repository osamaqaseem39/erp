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

    // Avoid Include on required AssignedToUser — missing User rows become INNER JOINs and hide activities.
    private IQueryable<Activity> ActivitiesQuery() =>
        ApplyTenant(Context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity));

    private async Task LoadAssigneeAsync(Activity? entity)
    {
        if (entity == null) return;
        await Context.Entry(entity).Reference(a => a.AssignedToUser).LoadAsync();
    }

    private async Task LoadAssigneesAsync(IEnumerable<Activity> entities)
    {
        foreach (var entity in entities)
            await Context.Entry(entity).Reference(a => a.AssignedToUser).LoadAsync();
    }

    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        var entity = await ActivitiesQuery().FirstOrDefaultAsync(a => a.Id == id);
        await LoadAssigneeAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync()
    {
        var list = await ActivitiesQuery().ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

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

    public async Task<IEnumerable<Activity>> GetByCustomerAsync(Guid customerId)
    {
        var list = await ActivitiesQuery().Where(a => a.CustomerId == customerId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId)
    {
        var list = await ActivitiesQuery().Where(a => a.LeadId == leadId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByOpportunityAsync(Guid opportunityId)
    {
        var list = await ActivitiesQuery().Where(a => a.OpportunityId == opportunityId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByAssignedToAsync(Guid assignedTo)
    {
        var list = await ActivitiesQuery().Where(a => a.AssignedToUserId == assignedTo).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByTypeAsync(string activityType)
    {
        var list = await ActivitiesQuery().Where(a => a.ActivityType == activityType).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var list = await ActivitiesQuery().Where(a => a.StartDate >= startDate && a.StartDate <= endDate).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId)
    {
        var list = await ActivitiesQuery()
            .Where(a => a.RelatedEntityType == entityType && a.RelatedEntityId == entityId)
            .ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }
}
