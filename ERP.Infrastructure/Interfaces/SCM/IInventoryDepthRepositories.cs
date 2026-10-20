using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Brand>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<Brand> CreateAsync(Brand entity);
    Task<bool> UpdateAsync(Brand entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IWarehouseLocationRepository
{
    Task<WarehouseLocation?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<WarehouseLocation>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<WarehouseLocation> CreateAsync(WarehouseLocation entity);
    Task<bool> UpdateAsync(WarehouseLocation entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IStockReservationRepository
{
    Task<StockReservation?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockReservation>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<StockReservation>> GetBySalesOrderAsync(Guid salesOrderId, Guid tenantId);
    Task<Guid> CreateAsync(StockReservation entity);
    Task<bool> UpdateAsync(StockReservation entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IItemKitRepository
{
    Task<ItemKit?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ItemKit>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(ItemKit entity);
    Task<bool> UpdateAsync(ItemKit entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ICycleCountRepository
{
    Task<CycleCount?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CycleCount>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CycleCount entity);
    Task<bool> UpdateAsync(CycleCount entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
