using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Services;

public class StockTransferApiService : IStockTransferApiService
{
    private readonly IStockTransferService _service;

    public StockTransferApiService(IStockTransferService service)
    {
        _service = service;
    }

    public Task<StockTransferDto?> GetByIdAsync(Guid id) =>
        _service.GetByIdAsync(id, Guid.Empty);

    public Task<IEnumerable<StockTransferDto>> GetAllAsync() =>
        _service.GetAllAsync(Guid.Empty);

    public Task<Guid> CreateAsync(CreateStockTransferDto dto) =>
        _service.CreateAsync(dto, dto.TenantId, Guid.TryParse(dto.CreatedBy, out var u) ? u : Guid.Empty);

    public Task<bool> UpdateAsync(UpdateStockTransferDto dto) =>
        _service.UpdateAsync(dto, Guid.Empty, Guid.TryParse(dto.UpdatedBy, out var u) ? u : Guid.Empty);

    public Task<bool> DeleteAsync(Guid id) =>
        _service.DeleteAsync(id, Guid.Empty);

    public Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId) =>
        _service.GetByFromWarehouseAsync(warehouseId, Guid.Empty);

    public Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId) =>
        _service.GetByToWarehouseAsync(warehouseId, Guid.Empty);

    public Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status) =>
        _service.GetByStatusAsync(status, Guid.Empty);

    public Task<bool> ApproveAsync(Guid id) =>
        _service.ApproveAsync(id, Guid.Empty, Guid.Empty);

    public Task<bool> CompleteAsync(Guid id) =>
        _service.CompleteAsync(id, Guid.Empty, Guid.Empty);
}
