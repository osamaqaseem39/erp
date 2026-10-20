using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface IFiscalYearService
{
    Task<Guid> CreateAsync(CreateFiscalYearDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateAsync(UpdateFiscalYearDto dto, Guid tenantId, Guid userId);
    Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<FiscalYearDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<FiscalYearDto>> GetAllAsync(Guid tenantId, string? status = null);
    Task<FiscalYearDto?> GetActiveCoveringAsync(Guid tenantId, DateTime date);
}
