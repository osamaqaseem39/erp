using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<IEnumerable<Company>> GetByTenantAsync(Guid tenantId);
    Task<Company?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Guid> CreateAsync(Company company);
    Task<bool> UpdateAsync(Company company);
    Task<bool> DeleteAsync(Guid id);
}