using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ERP.Infrastructure.Repositories.SCM;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IStoredProcedureExecutor _executor;

    public WarehouseRepository(IStoredProcedureExecutor executor)
    {
        _executor = executor;
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", id),
            new SqlParameter("@TenantId", tenantId)
        };

        var result = await _executor.ExecuteQueryAsync<DataRow>("getWarehouseById", parameters);
        return result.FirstOrDefault()?.ToWarehouse();
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(Guid tenantId, Guid companyId)
    {
        var parameters = new[]
        {
            new SqlParameter("@TenantId", tenantId),
            new SqlParameter("@CompanyId", companyId)
        };

        var result = await _executor.ExecuteQueryAsync<DataRow>("getAllWarehouses", parameters);
        return result.Select(r => r.ToWarehouse());
    }

    public async Task<Warehouse> AddAsync(Warehouse warehouse)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", warehouse.Id),
            new SqlParameter("@TenantId", warehouse.TenantId),
            new SqlParameter("@CompanyId", warehouse.CompanyId),
            new SqlParameter("@WarehouseCode", warehouse.WarehouseCode),
            new SqlParameter("@WarehouseName", warehouse.WarehouseName),
            new SqlParameter("@AddressLine1", warehouse.AddressLine1),
            new SqlParameter("@AddressLine2", warehouse.AddressLine2 ?? (object)DBNull.Value),
            new SqlParameter("@City", warehouse.City),
            new SqlParameter("@State", warehouse.State),
            new SqlParameter("@Country", warehouse.Country),
            new SqlParameter("@PostalCode", warehouse.PostalCode),
            new SqlParameter("@ContactPerson", warehouse.ContactPerson ?? (object)DBNull.Value),
            new SqlParameter("@PhoneNumber", warehouse.PhoneNumber ?? (object)DBNull.Value),
            new SqlParameter("@Email", warehouse.Email ?? (object)DBNull.Value),
            new SqlParameter("@CreatedBy", warehouse.CreatedBy ?? (object)DBNull.Value)
        };

        await _executor.ExecuteNonQueryAsync("addWarehouse", parameters);
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", warehouse.Id),
            new SqlParameter("@TenantId", warehouse.TenantId),
            new SqlParameter("@WarehouseCode", warehouse.WarehouseCode),
            new SqlParameter("@WarehouseName", warehouse.WarehouseName),
            new SqlParameter("@AddressLine1", warehouse.AddressLine1),
            new SqlParameter("@AddressLine2", warehouse.AddressLine2 ?? (object)DBNull.Value),
            new SqlParameter("@City", warehouse.City),
            new SqlParameter("@State", warehouse.State),
            new SqlParameter("@Country", warehouse.Country),
            new SqlParameter("@PostalCode", warehouse.PostalCode),
            new SqlParameter("@ContactPerson", warehouse.ContactPerson ?? (object)DBNull.Value),
            new SqlParameter("@PhoneNumber", warehouse.PhoneNumber ?? (object)DBNull.Value),
            new SqlParameter("@Email", warehouse.Email ?? (object)DBNull.Value),
            new SqlParameter("@IsActive", warehouse.IsActive),
            new SqlParameter("@UpdatedBy", warehouse.UpdatedBy ?? (object)DBNull.Value)
        };

        await _executor.ExecuteNonQueryAsync("updateWarehouse", parameters);
        return warehouse;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", id),
            new SqlParameter("@TenantId", tenantId)
        };

        var result = await _executor.ExecuteNonQueryAsync("deleteWarehouse", parameters);
        return result > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        var warehouse = await GetByIdAsync(id, tenantId);
        return warehouse != null;
    }

    public async Task<bool> CodeExistsAsync(string code, Guid tenantId, Guid companyId, Guid? excludeId = null)
    {
        var warehouses = await GetAllAsync(tenantId, companyId);
        return warehouses.Any(w => w.WarehouseCode.Equals(code, StringComparison.OrdinalIgnoreCase) && w.Id != excludeId);
    }
}

public static class WarehouseExtensions
{
    public static Warehouse ToWarehouse(this DataRow row)
    {
        return new Warehouse
        {
            Id = (Guid)row["Id"],
            TenantId = (Guid)row["TenantId"],
            CompanyId = (Guid)row["CompanyId"],
            WarehouseCode = row["WarehouseCode"].ToString()!,
            WarehouseName = row["WarehouseName"].ToString()!,
            AddressLine1 = row["AddressLine1"].ToString()!,
            AddressLine2 = row["AddressLine2"]?.ToString(),
            City = row["City"].ToString()!,
            State = row["State"].ToString()!,
            Country = row["Country"].ToString()!,
            PostalCode = row["PostalCode"].ToString()!,
            ContactPerson = row["ContactPerson"]?.ToString(),
            PhoneNumber = row["PhoneNumber"]?.ToString(),
            Email = row["Email"]?.ToString(),
            IsActive = (bool)row["IsActive"],
            CreatedAt = (DateTime)row["CreatedAt"],
            UpdatedAt = row["UpdatedAt"] as DateTime?,
            CreatedBy = row["CreatedBy"]?.ToString(),
            UpdatedBy = row["UpdatedBy"]?.ToString()
        };
    }
}