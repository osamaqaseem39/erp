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

    private IQueryable<Activity> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Activities.Where(_ => false);
        // Avoid Include on required AssignedToUser — missing User rows become INNER JOINs and hide activities.
        return Context.Activities
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.Opportunity)
            .Where(a => a.TenantId == tenantId);
    }

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

    public async Task<Activity?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await ForTenant(tenantId).FirstOrDefaultAsync(a => a.Id == id);
        await LoadAssigneeAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(Activity activity)
    {
        if (activity.TenantId == Guid.Empty) return Guid.Empty;
        if (activity.Id == Guid.Empty) activity.Id = Guid.NewGuid();
        if (activity.CreatedAt == default) activity.CreatedAt = DateTime.UtcNow;
        Context.Activities.Add(activity);
        await Context.SaveChangesAsync();
        return activity.Id;
    }

    public async Task<bool> UpdateAsync(Activity activity)
    {
        if (activity.TenantId == Guid.Empty) return false;
        activity.UpdatedAt = DateTime.UtcNow;
        Context.Activities.Update(activity);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var activity = await ForTenant(tenantId).FirstOrDefaultAsync(a => a.Id == id);
        if (activity == null) return false;
        Context.Activities.Remove(activity);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await ForTenant(tenantId).AnyAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId).Where(a => a.CustomerId == customerId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId).Where(a => a.LeadId == leadId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByOpportunityAsync(Guid opportunityId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId).Where(a => a.OpportunityId == opportunityId).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId).Where(a => a.AssignedToUserId == assignedTo).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByTypeAsync(string activityType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId).Where(a => a.ActivityType == activityType).ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId)
            .Where(a => a.StartDate >= startDate && a.StartDate <= endDate)
            .ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }

    public async Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await ForTenant(tenantId)
            .Where(a => a.RelatedEntityType == entityType && a.RelatedEntityId == entityId)
            .ToListAsync();
        await LoadAssigneesAsync(list);
        return list;
    }
}
