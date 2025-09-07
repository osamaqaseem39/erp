using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id);
    Task<IEnumerable<Campaign>> GetAllAsync();
    Task<Campaign> AddAsync(Campaign entity);
    Task UpdateAsync(Campaign entity);
    Task DeleteAsync(Guid id);
}