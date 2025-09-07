using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Services;

public class StockTransferApiService : IStockTransferApiService
{
    private readonly IStockTransferService _service;

    public StockTransferApiService(IStockTransferService service)
    {
        _service = service;
    }

    public async Task<StockTransferDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<StockTransferDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateStockTransferDto dto)
    {
        // Convert CreateStockTransferDto to StockTransferDto
        var stockTransferDto = new StockTransferDto
        {
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TransferDate = dto.TransferDate,
            Reference = dto.Reference,
            Notes = dto.Notes,
            Status = "Draft",
            Details = dto.Details?.Select(d => new StockTransferDetailDto
            {
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                Notes = d.Notes
            }).ToList() ?? new List<StockTransferDetailDto>()
        };
        return await _service.CreateAsync(stockTransferDto);
    }

    public async Task<bool> UpdateAsync(UpdateStockTransferDto dto)
    {
        // Convert UpdateStockTransferDto to StockTransferDto
        var stockTransferDto = new StockTransferDto
        {
            Id = dto.Id,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TransferDate = dto.TransferDate,
            Reference = dto.Reference,
            Notes = dto.Notes,
            Status = dto.Status,
            Details = dto.Details?.Select(d => new StockTransferDetailDto
            {
                Id = d.Id,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                Notes = d.Notes
            }).ToList() ?? new List<StockTransferDetailDto>()
        };
        return await _service.UpdateAsync(stockTransferDto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId)
    {
        // Use the available GetByWarehouseAsync method as a fallback
        return await _service.GetByWarehouseAsync(warehouseId);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId)
    {
        // Use the available GetByWarehouseAsync method as a fallback
        return await _service.GetByWarehouseAsync(warehouseId);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        // Note: IStockTransferService doesn't have ApproveAsync method
        // This would need to be implemented or use a different approach
        return false;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        // Note: IStockTransferService doesn't have CompleteAsync method
        // This would need to be implemented or use a different approach
        return false;
    }
}