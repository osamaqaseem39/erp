using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface ILandedCostVoucherService
{
    Task<LandedCostVoucherDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LandedCostVoucherDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateLandedCostVoucherDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateLandedCostVoucherDto dto, Guid tenantId, Guid userId);
    Task<bool> PostAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IVendorContractService
{
    Task<VendorContractDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<VendorContractDto>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<VendorContractDto?> GetActiveAsync(Guid vendorId, Guid? itemId, Guid tenantId);
    Task<VendorContractDto> CreateAsync(CreateVendorContractDto dto, Guid tenantId, Guid userId);
    Task<VendorContractDto?> UpdateAsync(UpdateVendorContractDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
