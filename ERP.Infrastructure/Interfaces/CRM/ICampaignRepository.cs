using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id);
    Task<IEnumerable<Campaign>> GetAllAsync();
    Task<IEnumerable<Campaign>> GetByStatusAsync(string status);
    Task<IEnumerable<Campaign>> GetByTypeAsync(string campaignType);
    Task<Guid> CreateAsync(Campaign campaign);
    Task<bool> UpdateAsync(Campaign campaign);
    Task<bool> DeleteAsync(Guid id);
}
