using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface ILandedCostVoucherRepository
{
    Task<LandedCostVoucher?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LandedCostVoucher>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(LandedCostVoucher entity);
    Task<bool> UpdateAsync(LandedCostVoucher entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IVendorContractRepository
{
    Task<VendorContract?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<VendorContract>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<VendorContract>> GetActiveForVendorItemAsync(Guid vendorId, Guid? itemId, DateTime asOf, Guid tenantId);
    Task<VendorContract> CreateAsync(VendorContract entity);
    Task<bool> UpdateAsync(VendorContract entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
