using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IInventoryValuationMethodService
{
    Task<InventoryValuationMethodDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<InventoryValuationMethodDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateInventoryValuationMethodDto dto);
    Task<bool> UpdateAsync(UpdateInventoryValuationMethodDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<InventoryValuationMethodDto>> GetActiveAsync();
}