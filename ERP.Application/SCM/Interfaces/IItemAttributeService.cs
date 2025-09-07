using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IItemAttributeService
{
    Task<IEnumerable<ItemAttributeDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<ItemAttributeDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemAttributeDto> CreateAsync(ItemAttributeDto dto, Guid tenantId, string createdBy);
    Task<ItemAttributeDto> UpdateAsync(ItemAttributeDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}