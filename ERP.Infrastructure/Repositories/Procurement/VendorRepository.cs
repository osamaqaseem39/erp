using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

/// <summary>
/// EF-backed vendor store so AuditSaveChangesInterceptor and TenantGuardInterceptor apply on writes.
/// </summary>
public class VendorRepository : IVendorRepository
{
    private readonly ERPDbContext _db;

    public VendorRepository(ERPDbContext db) => _db = db;

    private IQueryable<Vendor> Query(Guid tenantId) =>
        _db.Vendors.Where(v => v.TenantId == tenantId);

    public async Task<Vendor?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId)
            .Include(v => v.BankAccounts)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vendor?> GetByCodeAsync(string vendorCode, Guid companyId, Guid tenantId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(vendorCode)) return null;
        return await Query(tenantId)
            .FirstOrDefaultAsync(v => v.CompanyId == companyId
                                      && v.VendorCode == vendorCode);
    }

    public async Task<Vendor?> GetByEmailAsync(string email, Guid tenantId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(email)) return null;
        return await Query(tenantId)
            .FirstOrDefaultAsync(v => v.Email == email);
    }

    public async Task<IEnumerable<Vendor>> GetAllAsync(Guid tenantId, Guid? companyId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Vendor>();
        var q = Query(tenantId);
        if (companyId.HasValue && companyId != Guid.Empty)
            q = q.Where(v => v.CompanyId == companyId.Value);
        return await q.OrderBy(v => v.VendorName).ToListAsync();
    }

    public async Task<(IEnumerable<Vendor> Vendors, int TotalCount)> SearchAsync(
        Guid tenantId,
        Guid? companyId = null,
        string? searchTerm = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50,
        string sortBy = "VendorName",
        string sortDirection = "ASC")
    {
        if (tenantId == Guid.Empty)
            return (Array.Empty<Vendor>(), 0);

        var q = Query(tenantId);
        if (companyId.HasValue && companyId != Guid.Empty)
            q = q.Where(v => v.CompanyId == companyId.Value);
        if (isActive.HasValue)
            q = q.Where(v => v.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            q = q.Where(v =>
                v.VendorCode.Contains(term)
                || v.VendorName.Contains(term)
                || (v.Email != null && v.Email.Contains(term))
                || (v.PhoneNumber != null && v.PhoneNumber.Contains(term)));
        }

        var desc = string.Equals(sortDirection, "DESC", StringComparison.OrdinalIgnoreCase);
        q = (sortBy?.Trim() ?? "VendorName").ToLowerInvariant() switch
        {
            "vendorcode" => desc ? q.OrderByDescending(v => v.VendorCode) : q.OrderBy(v => v.VendorCode),
            "email" => desc ? q.OrderByDescending(v => v.Email) : q.OrderBy(v => v.Email),
            "createdat" => desc ? q.OrderByDescending(v => v.CreatedAt) : q.OrderBy(v => v.CreatedAt),
            _ => desc ? q.OrderByDescending(v => v.VendorName) : q.OrderBy(v => v.VendorName)
        };

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 50;

        var total = await q.CountAsync();
        var rows = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (rows, total);
    }

    public async Task<Guid> AddAsync(Vendor vendor)
    {
        if (vendor.Id == Guid.Empty) vendor.Id = Guid.NewGuid();
        if (vendor.CreatedAt == default) vendor.CreatedAt = DateTime.UtcNow;
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return vendor.Id;
    }

    public async Task<bool> UpdateAsync(Vendor vendor)
    {
        vendor.UpdatedAt ??= DateTime.UtcNow;
        _db.Vendors.Update(vendor);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.Vendors.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId) =>
        tenantId != Guid.Empty && await Query(tenantId).AnyAsync(v => v.Id == id);

    public async Task<bool> ExistsByCodeAsync(string vendorCode, Guid companyId, Guid tenantId, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(vendorCode)) return false;
        var q = Query(tenantId).Where(v => v.CompanyId == companyId && v.VendorCode == vendorCode);
        if (excludeId.HasValue) q = q.Where(v => v.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid tenantId, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(email)) return false;
        var q = Query(tenantId).Where(v => v.Email == email);
        if (excludeId.HasValue) q = q.Where(v => v.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}
