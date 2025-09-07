using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(Guid id, Guid tenantId);
    Task<Vendor?> GetByCodeAsync(string vendorCode, Guid companyId, Guid tenantId);
    Task<Vendor?> GetByEmailAsync(string email, Guid tenantId);
    Task<IEnumerable<Vendor>> GetAllAsync(Guid tenantId, Guid? companyId = null);
    Task<(IEnumerable<Vendor> Vendors, int TotalCount)> SearchAsync(
        Guid tenantId, 
        Guid? companyId = null, 
        string? searchTerm = null, 
        bool? isActive = null,
        int pageNumber = 1, 
        int pageSize = 50,
        string sortBy = "VendorName",
        string sortDirection = "ASC");
    Task<Guid> AddAsync(Vendor vendor);
    Task<bool> UpdateAsync(Vendor vendor);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<bool> ExistsByCodeAsync(string vendorCode, Guid companyId, Guid tenantId, Guid? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId, Guid? excludeId = null);
}