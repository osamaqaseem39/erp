using ERP.Domain.Procurement;
using ERP.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace ERP.Infrastructure.Repositories.Procurement;

public class VendorRepository : IVendorRepository
{
    private readonly IStoredProcedureExecutor _storedProcedureExecutor;

    public VendorRepository(IStoredProcedureExecutor storedProcedureExecutor)
    {
        _storedProcedureExecutor = storedProcedureExecutor;
    }

    public async Task<Vendor?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var parameters = new[]
        {
            new SqlParameter("@VendorID", SqlDbType.UniqueIdentifier) { Value = id },
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = tenantId }
        };

        var result = await _storedProcedureExecutor.ExecuteReaderAsync(
            "Procurement.sp_GetVendorById", parameters);

        if (result.HasRows)
        {
            await result.ReadAsync();
            return MapToVendor(result);
        }

        return null;
    }

    public async Task<Vendor?> GetByCodeAsync(string vendorCode, Guid companyId, Guid tenantId)
    {
        var parameters = new[]
        {
            new SqlParameter("@VendorCode", SqlDbType.NVarChar, 50) { Value = vendorCode },
            new SqlParameter("@CompanyID", SqlDbType.UniqueIdentifier) { Value = companyId },
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = tenantId }
        };

        var result = await _storedProcedureExecutor.ExecuteReaderAsync(
            "Procurement.sp_GetVendorByCode", parameters);

        if (result.HasRows)
        {
            await result.ReadAsync();
            return MapToVendor(result);
        }

        return null;
    }

    public async Task<Vendor?> GetByEmailAsync(string email, Guid tenantId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email },
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = tenantId }
        };

        var result = await _storedProcedureExecutor.ExecuteReaderAsync(
            "Procurement.sp_GetVendorByEmail", parameters);

        if (result.HasRows)
        {
            await result.ReadAsync();
            return MapToVendor(result);
        }

        return null;
    }

    public async Task<IEnumerable<Vendor>> GetAllAsync(Guid tenantId, Guid? companyId = null)
    {
        var parameters = new[]
        {
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = tenantId },
            new SqlParameter("@CompanyID", SqlDbType.UniqueIdentifier) { Value = companyId ?? (object)DBNull.Value }
        };

        var result = await _storedProcedureExecutor.ExecuteReaderAsync(
            "Procurement.sp_GetAllVendors", parameters);

        var vendors = new List<Vendor>();
        while (await result.ReadAsync())
        {
            vendors.Add(MapToVendor(result));
        }

        return vendors;
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
        var parameters = new[]
        {
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = tenantId },
            new SqlParameter("@CompanyID", SqlDbType.UniqueIdentifier) { Value = companyId ?? (object)DBNull.Value },
            new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 255) { Value = searchTerm ?? (object)DBNull.Value },
            new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive ?? (object)DBNull.Value },
            new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
            new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            new SqlParameter("@SortBy", SqlDbType.NVarChar, 50) { Value = sortBy },
            new SqlParameter("@SortDirection", SqlDbType.NVarChar, 4) { Value = sortDirection }
        };

        var result = await _storedProcedureExecutor.ExecuteReaderAsync(
            "Procurement.sp_GetAllVendors", parameters);

        var vendors = new List<Vendor>();
        while (await result.ReadAsync())
        {
            vendors.Add(MapToVendor(result));
        }

        // Get total count
        var totalCount = 0;
        if (await result.NextResultAsync())
        {
            await result.ReadAsync();
            totalCount = result.GetInt32("TotalCount");
        }

        return (vendors, totalCount);
    }

    public async Task<Guid> AddAsync(Vendor vendor)
    {
        var parameters = new[]
        {
            new SqlParameter("@CompanyID", SqlDbType.UniqueIdentifier) { Value = vendor.CompanyId },
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = vendor.TenantId },
            new SqlParameter("@VendorCode", SqlDbType.NVarChar, 50) { Value = vendor.VendorCode },
            new SqlParameter("@VendorName", SqlDbType.NVarChar, 255) { Value = vendor.VendorName },
            new SqlParameter("@ContactPerson", SqlDbType.NVarChar, 255) { Value = vendor.ContactPerson ?? (object)DBNull.Value },
            new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 50) { Value = vendor.PhoneNumber ?? (object)DBNull.Value },
            new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = vendor.Email ?? (object)DBNull.Value },
            new SqlParameter("@AddressLine1", SqlDbType.NVarChar, 255) { Value = vendor.AddressLine1 ?? (object)DBNull.Value },
            new SqlParameter("@AddressLine2", SqlDbType.NVarChar, 255) { Value = vendor.AddressLine2 ?? (object)DBNull.Value },
            new SqlParameter("@City", SqlDbType.NVarChar, 100) { Value = vendor.City ?? (object)DBNull.Value },
            new SqlParameter("@State", SqlDbType.NVarChar, 100) { Value = vendor.State ?? (object)DBNull.Value },
            new SqlParameter("@Country", SqlDbType.NVarChar, 100) { Value = vendor.Country ?? (object)DBNull.Value },
            new SqlParameter("@PostalCode", SqlDbType.NVarChar, 20) { Value = vendor.PostalCode ?? (object)DBNull.Value },
            new SqlParameter("@TaxNumber", SqlDbType.NVarChar, 50) { Value = vendor.TaxNumber ?? (object)DBNull.Value },
            new SqlParameter("@PaymentTerms", SqlDbType.NVarChar, 100) { Value = vendor.PaymentTerms ?? (object)DBNull.Value },
            new SqlParameter("@CreditLimit", SqlDbType.Decimal) { Value = vendor.CreditLimit, Precision = 18, Scale = 2 },
            new SqlParameter("@CreatedBy", SqlDbType.NVarChar, 255) { Value = vendor.CreatedBy ?? (object)DBNull.Value }
        };

        var result = await _storedProcedureExecutor.ExecuteScalarAsync<object>(
            "Procurement.sp_CreateVendor", parameters);

        return (Guid)result;
    }

    public async Task<bool> UpdateAsync(Vendor vendor)
    {
        var parameters = new[]
        {
            new SqlParameter("@VendorID", SqlDbType.UniqueIdentifier) { Value = vendor.Id },
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = vendor.TenantId },
            new SqlParameter("@VendorCode", SqlDbType.NVarChar, 50) { Value = vendor.VendorCode },
            new SqlParameter("@VendorName", SqlDbType.NVarChar, 255) { Value = vendor.VendorName },
            new SqlParameter("@ContactPerson", SqlDbType.NVarChar, 255) { Value = vendor.ContactPerson ?? (object)DBNull.Value },
            new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 50) { Value = vendor.PhoneNumber ?? (object)DBNull.Value },
            new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = vendor.Email ?? (object)DBNull.Value },
            new SqlParameter("@AddressLine1", SqlDbType.NVarChar, 255) { Value = vendor.AddressLine1 ?? (object)DBNull.Value },
            new SqlParameter("@AddressLine2", SqlDbType.NVarChar, 255) { Value = vendor.AddressLine2 ?? (object)DBNull.Value },
            new SqlParameter("@City", SqlDbType.NVarChar, 100) { Value = vendor.City ?? (object)DBNull.Value },
            new SqlParameter("@State", SqlDbType.NVarChar, 100) { Value = vendor.State ?? (object)DBNull.Value },
            new SqlParameter("@Country", SqlDbType.NVarChar, 100) { Value = vendor.Country ?? (object)DBNull.Value },
            new SqlParameter("@PostalCode", SqlDbType.NVarChar, 20) { Value = vendor.PostalCode ?? (object)DBNull.Value },
            new SqlParameter("@TaxNumber", SqlDbType.NVarChar, 50) { Value = vendor.TaxNumber ?? (object)DBNull.Value },
            new SqlParameter("@PaymentTerms", SqlDbType.NVarChar, 100) { Value = vendor.PaymentTerms ?? (object)DBNull.Value },
            new SqlParameter("@CreditLimit", SqlDbType.Decimal) { Value = vendor.CreditLimit, Precision = 18, Scale = 2 },
            new SqlParameter("@IsActive", SqlDbType.Bit) { Value = vendor.IsActive },
            new SqlParameter("@UpdatedBy", SqlDbType.NVarChar, 255) { Value = vendor.UpdatedBy ?? (object)DBNull.Value }
        };

        var result = await _storedProcedureExecutor.ExecuteNonQueryAsync(
            "Procurement.sp_UpdateVendor", parameters);

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var parameters = new[]
        {
            new SqlParameter("@VendorID", SqlDbType.UniqueIdentifier) { Value = id },
            new SqlParameter("@TenantID", SqlDbType.UniqueIdentifier) { Value = tenantId }
        };

        var result = await _storedProcedureExecutor.ExecuteNonQueryAsync(
            "Procurement.sp_DeleteVendor", parameters);

        return result > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        var vendor = await GetByIdAsync(id, tenantId);
        return vendor != null;
    }

    public async Task<bool> ExistsByCodeAsync(string vendorCode, Guid companyId, Guid tenantId, Guid? excludeId = null)
    {
        var vendor = await GetByCodeAsync(vendorCode, companyId, tenantId);
        return vendor != null && (excludeId == null || vendor.Id != excludeId);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid tenantId, Guid? excludeId = null)
    {
        var vendor = await GetByEmailAsync(email, tenantId);
        return vendor != null && (excludeId == null || vendor.Id != excludeId);
    }

    private static Vendor MapToVendor(DbDataReader reader)
    {
        return new Vendor
        {
            Id = reader.GetGuid("VendorID"),
            TenantId = reader.GetGuid("TenantID"),
            CompanyId = reader.GetGuid("CompanyID"),
            VendorCode = reader.GetString("VendorCode"),
            VendorName = reader.GetString("VendorName"),
            ContactPerson = reader.IsDBNull("ContactPerson") ? string.Empty : reader.GetString("ContactPerson"),
            PhoneNumber = reader.IsDBNull("PhoneNumber") ? string.Empty : reader.GetString("PhoneNumber"),
            Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString("Email"),
            AddressLine1 = reader.IsDBNull("AddressLine1") ? string.Empty : reader.GetString("AddressLine1"),
            AddressLine2 = reader.IsDBNull("AddressLine2") ? string.Empty : reader.GetString("AddressLine2"),
            City = reader.IsDBNull("City") ? string.Empty : reader.GetString("City"),
            State = reader.IsDBNull("State") ? string.Empty : reader.GetString("State"),
            Country = reader.IsDBNull("Country") ? string.Empty : reader.GetString("Country"),
            PostalCode = reader.IsDBNull("PostalCode") ? string.Empty : reader.GetString("PostalCode"),
            TaxNumber = reader.IsDBNull("TaxNumber") ? string.Empty : reader.GetString("TaxNumber"),
            PaymentTerms = reader.IsDBNull("PaymentTerms") ? string.Empty : reader.GetString("PaymentTerms"),
            CreditLimit = reader.GetDecimal("CreditLimit"),
            IsActive = reader.GetBoolean("IsActive"),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            UpdatedAt = reader.IsDBNull("UpdatedAt") ? null : reader.GetDateTime("UpdatedAt"),
            CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetString("CreatedBy"),
            UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetString("UpdatedBy")
        };
    }
}