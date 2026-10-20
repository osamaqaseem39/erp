using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ICampaignService
{
    Task<CampaignDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CampaignDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateCampaignDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateCampaignDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CampaignDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<CampaignDto>> GetByTypeAsync(string campaignType, Guid tenantId);
    Task<IEnumerable<CampaignDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> LaunchAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> PauseAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}
