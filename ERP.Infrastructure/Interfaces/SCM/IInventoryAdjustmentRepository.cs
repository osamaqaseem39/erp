using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IInventoryAdjustmentRepository
{
    Task<InventoryAdjustment?> GetByIdAsync(int id);
    Task<IEnumerable<InventoryAdjustment>> GetAllAsync();
    Task<InventoryAdjustment> AddAsync(InventoryAdjustment entity);
    Task UpdateAsync(InventoryAdjustment entity);
    Task DeleteAsync(int id);
}