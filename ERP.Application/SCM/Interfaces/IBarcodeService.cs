using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IBarcodeService
{
    Task<IEnumerable<BarcodeDto>> GetByItemAsync(Guid? itemId, Guid? variantId, Guid tenantId);
    Task<BarcodeDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<BarcodeDto> CreateAsync(BarcodeDto dto, Guid tenantId, string createdBy);
    Task<BarcodeDto> UpdateAsync(BarcodeDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}