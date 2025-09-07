using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class CampaignRepository : ICampaignRepository
{
    private readonly ERPDbContext _context;

    public CampaignRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Campaign?> GetByIdAsync(Guid id)
    {
        return await _context.Campaigns.FindAsync(id);
    }

    public async Task<IEnumerable<Campaign>> GetAllAsync()
    {
        return await _context.Campaigns.ToListAsync();
    }

    public async Task<IEnumerable<Campaign>> GetByStatusAsync(string status)
    {
        return await _context.Campaigns
            .Where(c => c.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Campaign>> GetByTypeAsync(string campaignType)
    {
        return await _context.Campaigns
            .Where(c => c.CampaignType == campaignType)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Campaign campaign)
    {
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();
        return campaign.Id;
    }

    public async Task<bool> UpdateAsync(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Campaigns.Remove(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }
}