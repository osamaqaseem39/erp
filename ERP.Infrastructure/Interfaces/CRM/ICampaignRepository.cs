using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Campaign>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Campaign>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<Campaign>> GetByTypeAsync(string campaignType, Guid tenantId);
    Task<Guid> CreateAsync(Campaign campaign);
    Task<bool> UpdateAsync(Campaign campaign);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
