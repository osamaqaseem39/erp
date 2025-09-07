using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IItemGroupService
{
    Task<IEnumerable<ItemGroupDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<ItemGroupDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemGroupDto> CreateAsync(ItemGroupDto dto, Guid tenantId, string createdBy);
    Task<ItemGroupDto> UpdateAsync(ItemGroupDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}