using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class OpportunityRepository : CrmRepositoryBase, IOpportunityRepository
{
    public OpportunityRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Opportunity> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Opportunities.Where(_ => false);
        return Context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Lead)
            .Include(o => o.AssignedToUser)
            .Include(o => o.Activities)
            .Where(o => o.TenantId == tenantId);
    }

    public async Task<Opportunity?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Opportunity>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(o => o.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(o => o.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByStageAsync(string stage, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(o => o.Stage == stage).ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId)
            .Where(o => o.AssignedTo == assignedTo || o.AssignedToUserId == assignedTo)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await ForTenant(tenantId).AnyAsync(o => o.Id == id);
    }

    public async Task<Opportunity?> GetByCodeAsync(string code, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(o => o.OpportunityCode == code);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid tenantId, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty) return false;
        var query = ForTenant(tenantId).Where(o => o.OpportunityCode == code);
        if (excludeId.HasValue)
            query = query.Where(o => o.Id != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<Guid> CreateAsync(Opportunity opportunity)
    {
        if (opportunity.TenantId == Guid.Empty) return Guid.Empty;
        if (opportunity.Id == Guid.Empty) opportunity.Id = Guid.NewGuid();
        if (opportunity.CreatedAt == default) opportunity.CreatedAt = DateTime.UtcNow;
        Context.Opportunities.Add(opportunity);
        await Context.SaveChangesAsync();
        return opportunity.Id;
    }

    public async Task<bool> UpdateAsync(Opportunity opportunity)
    {
        if (opportunity.TenantId == Guid.Empty) return false;
        opportunity.UpdatedAt = DateTime.UtcNow;
        Context.Opportunities.Update(opportunity);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var opportunity = await ForTenant(tenantId).FirstOrDefaultAsync(o => o.Id == id);
        if (opportunity == null) return false;
        Context.Opportunities.Remove(opportunity);
        return await Context.SaveChangesAsync() > 0;
    }
}
