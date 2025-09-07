using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface ICustomerApiService : IApiService<CustomerDto, CreateCustomerDto, UpdateCustomerDto>
{
    Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm);
    Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status);
    Task<IEnumerable<CustomerDto>> GetByIndustryAsync(string industry);
}