using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class CampaignRepository : CrmRepositoryBase, ICampaignRepository
{
    public CampaignRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Campaign> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Campaigns.Where(_ => false);
        return Context.Campaigns.Where(c => c.TenantId == tenantId);
    }

    public async Task<Campaign?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Campaign>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Campaign>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(c => c.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Campaign>> GetByTypeAsync(string campaignType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(c => c.CampaignType == campaignType).ToListAsync();
    }

    public async Task<Guid> CreateAsync(Campaign campaign)
    {
        if (campaign.TenantId == Guid.Empty) return Guid.Empty;
        if (campaign.Id == Guid.Empty) campaign.Id = Guid.NewGuid();
        if (campaign.CreatedAt == default) campaign.CreatedAt = DateTime.UtcNow;
        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();
        return campaign.Id;
    }

    public async Task<bool> UpdateAsync(Campaign campaign)
    {
        if (campaign.TenantId == Guid.Empty) return false;
        campaign.UpdatedAt = DateTime.UtcNow;
        Context.Campaigns.Update(campaign);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await ForTenant(tenantId).FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null) return false;
        Context.Campaigns.Remove(entity);
        return await Context.SaveChangesAsync() > 0;
    }
}
