using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IWarehouseService
{
    Task<WarehouseDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<WarehouseDto>> GetAllAsync(Guid tenantId, Guid companyId);
    Task<IEnumerable<WarehouseDto>> SearchAsync(string searchTerm, Guid tenantId, Guid companyId = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, Guid tenantId, string createdBy);
    Task<WarehouseDto> UpdateAsync(UpdateWarehouseDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<bool> ActivateAsync(Guid id, Guid tenantId, string updatedBy);
    Task<bool> DeactivateAsync(Guid id, Guid tenantId, string updatedBy);
}
