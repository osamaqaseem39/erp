using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseRequisitionService
{
    Task<PurchaseRequisitionDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseRequisitionDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePurchaseRequisitionDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePurchaseRequisitionDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseRequisitionDto>> GetByRequesterAsync(Guid requesterId, Guid tenantId);
    Task<IEnumerable<PurchaseRequisitionDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<PurchaseRequisitionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
}
