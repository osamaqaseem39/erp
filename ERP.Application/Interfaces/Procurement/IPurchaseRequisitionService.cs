using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseRequisitionService
{
    Task<PurchaseRequisitionDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseRequisitionDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreatePurchaseRequisitionDto dto);
    Task<bool> UpdateAsync(UpdatePurchaseRequisitionDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PurchaseRequisitionDto>> GetByRequesterAsync(Guid requesterId);
    Task<IEnumerable<PurchaseRequisitionDto>> GetByStatusAsync(string status);
    Task<IEnumerable<PurchaseRequisitionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id);
}