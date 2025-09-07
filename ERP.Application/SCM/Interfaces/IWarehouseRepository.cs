using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Warehouse>> GetAllAsync(Guid tenantId, Guid companyId);
    Task<Warehouse> AddAsync(Warehouse warehouse);
    Task<Warehouse> UpdateAsync(Warehouse warehouse);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<bool> CodeExistsAsync(string code, Guid tenantId, Guid companyId, Guid? excludeId = null);
}