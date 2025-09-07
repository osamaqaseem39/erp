using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IBarcodeRepository
{
    Task<IEnumerable<Barcode>> GetByItemAsync(Guid? itemId, Guid? variantId, Guid tenantId);
    Task<Barcode?> GetByIdAsync(Guid id, Guid tenantId);
    Task<Barcode> CreateAsync(Barcode entity);
    Task<Barcode> UpdateAsync(Barcode entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}