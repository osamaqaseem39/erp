using ERP.Application.Interfaces;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class CustomerApiService : ICustomerApiService
{
    private readonly ICustomerService _service;

    public CustomerApiService(ICustomerService service)
    {
        _service = service;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateCustomerDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateCustomerDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm)
    {
        return await _service.SearchAsync(searchTerm);
    }

    public async Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<CustomerDto>> GetByIndustryAsync(string industry)
    {
        return await _service.GetByIndustryAsync(industry);
    }
}