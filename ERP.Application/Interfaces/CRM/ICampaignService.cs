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
    Task<bool> LaunchAsync(Guid id);
    Task<bool> PauseAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}