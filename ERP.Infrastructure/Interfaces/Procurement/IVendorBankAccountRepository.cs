using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IVendorBankAccountRepository
{
    Task<VendorBankAccount?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<VendorBankAccount>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<Guid> CreateAsync(VendorBankAccount entity);
    Task<bool> UpdateAsync(VendorBankAccount entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
