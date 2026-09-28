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

    private IQueryable<Campaign> CampaignsQuery() => ApplyTenant(Context.Campaigns);

    public async Task<Campaign?> GetByIdAsync(Guid id) =>
        await CampaignsQuery().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Campaign>> GetAllAsync() =>
        await CampaignsQuery().ToListAsync();

    public async Task<IEnumerable<Campaign>> GetByStatusAsync(string status) =>
        await CampaignsQuery().Where(c => c.Status == status).ToListAsync();

    public async Task<IEnumerable<Campaign>> GetByTypeAsync(string campaignType) =>
        await CampaignsQuery().Where(c => c.CampaignType == campaignType).ToListAsync();

    public async Task<Guid> CreateAsync(Campaign campaign)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            campaign.TenantId = TenantId.Value;

        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();
        return campaign.Id;
    }

    public async Task<bool> UpdateAsync(Campaign campaign)
    {
        if (!MatchesTenant(campaign)) return false;
        Context.Campaigns.Update(campaign);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        Context.Campaigns.Remove(entity);
        return await Context.SaveChangesAsync() > 0;
    }
}
