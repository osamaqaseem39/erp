using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class ActivityApiService : IActivityApiService
{
    private readonly IActivityService _service;

    public ActivityApiService(IActivityService service)
    {
        _service = service;
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ActivityDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateActivityDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateActivityDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCustomerAsync(Guid customerId)
    {
        return await _service.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLeadAsync(Guid leadId)
    {
        return await _service.GetByLeadAsync(leadId);
    }

    public async Task<IEnumerable<ActivityDto>> GetByOpportunityAsync(Guid opportunityId)
    {
        return await _service.GetByOpportunityAsync(opportunityId);
    }

    public async Task<IEnumerable<ActivityDto>> GetByAssignedToAsync(Guid assignedTo)
    {
        return await _service.GetByAssignedToAsync(assignedTo);
    }

    public async Task<IEnumerable<ActivityDto>> GetByTypeAsync(string activityType)
    {
        return await _service.GetByTypeAsync(activityType);
    }

    public async Task<IEnumerable<ActivityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _service.GetByDateRangeAsync(startDate, endDate);
    }
}