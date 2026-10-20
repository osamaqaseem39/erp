using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ICampaignService
{
    Task<CampaignDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CampaignDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateCampaignDto dto);
    Task<bool> UpdateAsync(UpdateCampaignDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<CampaignDto>> GetByStatusAsync(string status);
    Task<IEnumerable<CampaignDto>> GetByTypeAsync(string campaignType);
    Task<IEnumerable<CampaignDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> LaunchAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> PauseAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}
