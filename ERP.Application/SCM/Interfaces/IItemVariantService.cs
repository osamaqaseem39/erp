using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IItemVariantService
{
    Task<IEnumerable<ItemVariantDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ItemVariantDto>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<ItemVariantDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemVariantDto> CreateAsync(ItemVariantDto dto, Guid tenantId, string createdBy);
    Task<ItemVariantDto> UpdateAsync(ItemVariantDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}