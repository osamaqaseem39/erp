using ERP.Application.Interfaces;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class LeadApiService : ILeadApiService
{
    private readonly ILeadService _service;

    public LeadApiService(ILeadService service)
    {
        _service = service;
    }

    public async Task<LeadDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<LeadDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateLeadDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateLeadDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<LeadDto>> GetBySourceAsync(string source)
    {
        return await _service.GetBySourceAsync(source);
    }

    public async Task<IEnumerable<LeadDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<LeadDto>> GetByAssignedToAsync(Guid assignedTo)
    {
        // Note: ILeadService doesn't have GetByAssignedToAsync method
        // This would need to be implemented or use a different approach
        return await _service.GetAllAsync();
    }

    public async Task<Guid> ConvertToCustomerAsync(Guid id)
    {
        return await _service.ConvertToCustomerAsync(id);
    }
}