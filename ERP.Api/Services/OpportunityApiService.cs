using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class OpportunityApiService : IOpportunityApiService
{
    private readonly IOpportunityService _service;

    public OpportunityApiService(IOpportunityService service)
    {
        _service = service;
    }

    public async Task<OpportunityDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<OpportunityDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateOpportunityDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateOpportunityDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByCustomerAsync(Guid customerId)
    {
        return await _service.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByAssignedToAsync(Guid assignedTo)
    {
        return await _service.GetByAssignedToAsync(assignedTo);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByStageAsync(string stage)
    {
        return await _service.GetByStageAsync(stage);
    }

    public async Task<bool> ConvertToQuoteAsync(Guid id)
    {
        return await _service.ConvertToQuoteAsync(id);
    }
}