using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseReturnService
{
    Task<PurchaseReturnDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseReturnDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePurchaseReturnDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePurchaseReturnDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}
