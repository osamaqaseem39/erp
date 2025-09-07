using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IWarehouseService
{
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<WarehouseDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateWarehouseDto dto);
    Task<bool> UpdateAsync(UpdateWarehouseDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<WarehouseDto>> GetActiveAsync();
    Task<IEnumerable<WarehouseDto>> SearchAsync(string searchTerm);
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
}