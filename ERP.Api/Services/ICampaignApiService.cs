using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface ICampaignApiService : IApiService<CampaignDto, CreateCampaignDto, UpdateCampaignDto>
{
    Task<IEnumerable<CampaignDto>> GetByStatusAsync(string status);
    Task<IEnumerable<CampaignDto>> GetByTypeAsync(string campaignType);
    Task<IEnumerable<CampaignDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> LaunchAsync(Guid id);
    Task<bool> PauseAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}