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

    private IQueryable<Opportunity> OpportunitiesQuery() =>
        ApplyTenant(Context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities));

    public async Task<Opportunity?> GetByIdAsync(Guid id) =>
        await OpportunitiesQuery().FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Opportunity>> GetAllAsync() =>
        await OpportunitiesQuery().ToListAsync();

    public async Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId) =>
        await OpportunitiesQuery().Where(o => o.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Opportunity>> GetByStatusAsync(string status) =>
        await OpportunitiesQuery().Where(o => o.Status == status).ToListAsync();

    public async Task<IEnumerable<Opportunity>> GetByStageAsync(string stage) =>
        await OpportunitiesQuery().Where(o => o.Stage == stage).ToListAsync();

    public async Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo) =>
        await OpportunitiesQuery().Where(o => o.AssignedTo == assignedTo || o.AssignedToUserId == assignedTo).ToListAsync();

    public async Task<bool> ExistsAsync(Guid id) =>
        await OpportunitiesQuery().AnyAsync(o => o.Id == id);

    public async Task<Opportunity?> GetByCodeAsync(string code) =>
        await OpportunitiesQuery().FirstOrDefaultAsync(o => o.OpportunityCode == code);

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var query = OpportunitiesQuery().Where(o => o.OpportunityCode == code);
        if (excludeId.HasValue)
            query = query.Where(o => o.Id != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<Guid> CreateAsync(Opportunity opportunity)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            opportunity.TenantId = TenantId.Value;

        Context.Opportunities.Add(opportunity);
        await Context.SaveChangesAsync();
        return opportunity.Id;
    }

    public async Task<bool> UpdateAsync(Opportunity opportunity)
    {
        if (!MatchesTenant(opportunity)) return false;
        Context.Opportunities.Update(opportunity);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var opportunity = await GetByIdAsync(id);
        if (opportunity == null) return false;
        Context.Opportunities.Remove(opportunity);
        return await Context.SaveChangesAsync() > 0;
    }
}
