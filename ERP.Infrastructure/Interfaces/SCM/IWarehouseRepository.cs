using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id);
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<Warehouse> AddAsync(Warehouse entity);
    Task UpdateAsync(Warehouse entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Warehouse>> GetByCompanyIdAsync(Guid companyId);
    Task<Warehouse?> GetByCodeAsync(string code);
}