using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IBrandService
{
    Task<BrandDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BrandDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<BrandDto> CreateAsync(CreateBrandDto dto, Guid tenantId, Guid userId);
    Task<BrandDto?> UpdateAsync(UpdateBrandDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IWarehouseLocationService
{
    Task<WarehouseLocationDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<WarehouseLocationDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<WarehouseLocationDto> CreateAsync(CreateWarehouseLocationDto dto, Guid tenantId, Guid userId);
    Task<WarehouseLocationDto?> UpdateAsync(UpdateWarehouseLocationDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IStockReservationService
{
    Task<StockReservationDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockReservationDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<StockReservationDto>> GetBySalesOrderAsync(Guid salesOrderId, Guid tenantId);
    Task<Guid> CreateAsync(CreateStockReservationDto dto, Guid tenantId, Guid userId);
    Task<bool> ReleaseAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IItemKitService
{
    Task<ItemKitDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemKitDto?> GetByKitItemIdAsync(Guid kitItemId, Guid tenantId);
    Task<IEnumerable<ItemKitDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateItemKitDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateItemKitDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ICycleCountService
{
    Task<CycleCountDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CycleCountDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateCycleCountDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateCycleCountDto dto, Guid tenantId, Guid userId);
    Task<bool> MarkCountedAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> PostAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPutawayTaskService
{
    Task<PutawayTaskDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PutawayTaskDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePutawayTaskDto dto, Guid tenantId, Guid userId);
    Task<Guid> CreateFromGrnAsync(Guid grnId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, Guid tenantId, Guid userId);
    Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, CompletePutawayTaskDto? dto, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPickListService
{
    Task<PickListDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PickListDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePickListDto dto, Guid tenantId, Guid userId);
    Task<Guid> CreateFromDispatchAsync(Guid dispatchId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, Guid tenantId, Guid userId);
    Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, CompletePickListDto? dto, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> HasCompletedPickForDispatchAsync(Guid dispatchId, Guid tenantId);
}

public interface IReplenishmentTaskService
{
    Task<ReplenishmentTaskDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ReplenishmentTaskDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateReplenishmentTaskDto dto, Guid tenantId, Guid userId);
    Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPackOrderService
{
    Task<PackOrderDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PackOrderDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePackOrderDto dto, Guid tenantId, Guid userId);
    Task<Guid> CreateFromPickAsync(Guid pickListId, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> HasPackedForDispatchAsync(Guid dispatchId, Guid tenantId);
}

public interface IShipmentService
{
    Task<ShipmentDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ShipmentDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateShipmentDto dto, Guid tenantId, Guid userId);
    Task<Guid> CreateFromPackAsync(Guid packOrderId, Guid tenantId, Guid userId);
    Task<bool> ShipAsync(Guid id, ShipShipmentDto? dto, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> HasShippedForDispatchAsync(Guid dispatchId, Guid tenantId);
}
