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
    Task<ItemKit?> GetByKitItemIdAsync(Guid kitItemId, Guid tenantId);
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

public interface IPutawayTaskRepository
{
    Task<PutawayTask?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PutawayTask>> GetAllAsync(Guid tenantId);
    Task<PutawayTask?> GetByReferenceAsync(string referenceDocType, Guid referenceDocId, Guid tenantId);
    Task<Guid> CreateAsync(PutawayTask entity);
    Task<bool> UpdateAsync(PutawayTask entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPickListRepository
{
    Task<PickList?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PickList>> GetAllAsync(Guid tenantId);
    Task<PickList?> GetCompletedByReferenceAsync(string referenceDocType, Guid referenceDocId, Guid tenantId);
    Task<IEnumerable<PickList>> GetByReferenceAsync(string referenceDocType, Guid referenceDocId, Guid tenantId);
    Task<Guid> CreateAsync(PickList entity);
    Task<bool> UpdateAsync(PickList entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IReplenishmentTaskRepository
{
    Task<ReplenishmentTask?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ReplenishmentTask>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(ReplenishmentTask entity);
    Task<bool> UpdateAsync(ReplenishmentTask entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPackOrderRepository
{
    Task<PackOrder?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PackOrder>> GetAllAsync(Guid tenantId);
    Task<PackOrder?> GetPackedByDispatchAsync(Guid dispatchAdviceId, Guid tenantId);
    Task<Guid> CreateAsync(PackOrder entity);
    Task<bool> UpdateAsync(PackOrder entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Shipment>> GetAllAsync(Guid tenantId);
    Task<Shipment?> GetShippedByDispatchAsync(Guid dispatchAdviceId, Guid tenantId);
    Task<Guid> CreateAsync(Shipment entity);
    Task<bool> UpdateAsync(Shipment entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
