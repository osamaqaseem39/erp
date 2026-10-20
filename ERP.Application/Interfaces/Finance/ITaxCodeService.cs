using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface ITaxCodeService
{
    Task<TaxCodeDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TaxCodeDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<TaxCodeDto>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<TaxCodeDto>> GetByTypeAsync(string taxType, Guid tenantId);
    Task<Guid> CreateAsync(CreateTaxCodeDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateTaxCodeDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
