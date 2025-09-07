using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface ICompanyService
{
    Task<CompanyDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<IEnumerable<CompanyDto>> GetByTenantAsync(Guid tenantId);
    Task<CompanyDto?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Guid> CreateAsync(CreateCompanyDto dto);
    Task<bool> UpdateAsync(UpdateCompanyDto dto);
    Task<bool> DeleteAsync(Guid id);
}