using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IVendorService
{
    Task<Guid> AddVendorAsync(CreateVendorDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateVendorAsync(UpdateVendorDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteVendorAsync(Guid vendorId, Guid tenantId);
    Task<VendorDto?> GetVendorByIdAsync(Guid vendorId, Guid tenantId);
    Task<(IEnumerable<VendorDto> Vendors, int TotalCount)> SearchVendorsAsync(VendorQuery query);
    Task<Guid> AddVendorBankAccountAsync(CreateVendorBankAccountDto dto, Guid vendorId, Guid tenantId, Guid userId);
    Task<Guid> UpdateVendorBankAccountAsync(UpdateVendorBankAccountDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteVendorBankAccountAsync(Guid bankAccountId, Guid tenantId);
    Task<IEnumerable<VendorBankAccountDto>> GetVendorBankAccountsAsync(Guid vendorId, Guid tenantId);
    Task<bool> ValidateVendorCodeAsync(string vendorCode, Guid companyId, Guid tenantId, Guid? excludeVendorId = null);
    Task<bool> ValidateVendorEmailAsync(string email, Guid tenantId, Guid? excludeVendorId = null);
}