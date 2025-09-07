using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IInventoryValuationMethodService
{
    Task<IEnumerable<InventoryValuationMethodDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<InventoryValuationMethodDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<InventoryValuationMethodDto> CreateAsync(InventoryValuationMethodDto dto, Guid tenantId, string createdBy);
    Task<InventoryValuationMethodDto> UpdateAsync(InventoryValuationMethodDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}