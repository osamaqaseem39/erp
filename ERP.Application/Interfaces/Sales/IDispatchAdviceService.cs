using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Interfaces.Sales;

public interface IDispatchAdviceService
{
    Task<DispatchAdviceDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<DispatchAdviceDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateDispatchAdviceDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateDispatchAdviceDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DispatchAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}
