using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IBarcodeService
{
    Task<BarcodeDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BarcodeDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateBarcodeDto dto);
    Task<bool> UpdateAsync(UpdateBarcodeDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<BarcodeDto?> GetByCodeAsync(string barcode);
    Task<IEnumerable<BarcodeDto>> GetByItemAsync(Guid itemId);
}