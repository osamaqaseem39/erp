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
