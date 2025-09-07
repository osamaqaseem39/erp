using ERP.Application.Interfaces;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class CampaignApiService : ICampaignApiService
{
    private readonly ICampaignService _service;

    public CampaignApiService(ICampaignService service)
    {
        _service = service;
    }

    public async Task<CampaignDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<CampaignDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateCampaignDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateCampaignDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<CampaignDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<CampaignDto>> GetByTypeAsync(string campaignType)
    {
        return await _service.GetByTypeAsync(campaignType);
    }

    public async Task<IEnumerable<CampaignDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _service.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> LaunchAsync(Guid id)
    {
        return await _service.LaunchAsync(id);
    }

    public async Task<bool> PauseAsync(Guid id)
    {
        return await _service.PauseAsync(id);
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        return await _service.CompleteAsync(id);
    }
}