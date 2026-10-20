using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using IAppStockBalanceRepository = ERP.Infrastructure.Interfaces.SCM.IStockBalanceRepository;
using IStockService = ERP.Application.Interfaces.IStockService;
using IStockTransferService = ERP.Application.Interfaces.Inventory.IStockTransferService;
using IItemService = ERP.Application.SCM.Interfaces.IItemService;
using IInventoryService = ERP.Application.SCM.Interfaces.IInventoryService;

namespace ERP.Application.SCM.Services;

/// <summary>Facade over stock balance/ledger/transfer services for GraphQL + /api/inventory.</summary>
public class InventoryService : IInventoryService
{
    private readonly IStockService _stock;
    private readonly IStockTransferService _transfers;
    private readonly IAppStockBalanceRepository _balances;
    private readonly IItemService _items;

    public InventoryService(
        IStockService stock,
        IStockTransferService transfers,
        IAppStockBalanceRepository balances,
        IItemService items)
    {
        _stock = stock;
        _transfers = transfers;
        _balances = balances;
        _items = items;
    }

    public async Task<StockBalanceDto?> GetStockBalanceAsync(Guid itemId, Guid warehouseId, Guid tenantId)
    {
        if (tenantId == Guid.Empty || itemId == Guid.Empty || warehouseId == Guid.Empty) return null;
        var entity = await _balances.GetByItemAndWarehouseAsync(itemId, warehouseId, tenantId);
        return entity == null ? null : MapBalance(entity);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetAllStockBalancesAsync(
        Guid tenantId, Guid companyId, Guid? warehouseId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockBalanceDto>();
        IEnumerable<Domain.SCM.StockBalance> rows = warehouseId.HasValue && warehouseId != Guid.Empty
            ? await _balances.GetByWarehouseAsync(warehouseId.Value, tenantId)
            : await _balances.GetAllAsync(tenantId);
        return rows.Select(MapBalance);
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(
        Guid itemId, Guid warehouseId, Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty || itemId == Guid.Empty) return Array.Empty<StockLedgerDto>();
        var ledgers = await _stock.GetStockLedgerAsync(itemId, tenantId, warehouseId, toDate);
        if (fromDate.HasValue)
            ledgers = ledgers.Where(l => l.TransactionDate >= fromDate.Value);
        return ledgers;
    }

    public async Task<bool> AdjustInventoryAsync(
        Guid itemId, Guid warehouseId, Guid tenantId, string adjustmentType, decimal quantity,
        string reason, string? approvedBy, string createdBy)
    {
        if (tenantId == Guid.Empty || itemId == Guid.Empty || warehouseId == Guid.Empty || quantity == 0)
            return false;

        var signed = quantity;
        var type = (adjustmentType ?? string.Empty).Trim();
        if (type.Equals("Decrease", StringComparison.OrdinalIgnoreCase)
            || type.Equals("Issue", StringComparison.OrdinalIgnoreCase)
            || type.Equals("Out", StringComparison.OrdinalIgnoreCase))
            signed = -Math.Abs(quantity);
        else if (type.Equals("Increase", StringComparison.OrdinalIgnoreCase)
                 || type.Equals("Receipt", StringComparison.OrdinalIgnoreCase)
                 || type.Equals("In", StringComparison.OrdinalIgnoreCase))
            signed = Math.Abs(quantity);

        var dto = new StockAdjustmentDto
        {
            TenantId = tenantId,
            WarehouseId = warehouseId,
            AdjustmentType = string.IsNullOrWhiteSpace(type) ? "Physical" : type,
            Notes = reason,
            CreatedBy = createdBy,
            UpdatedBy = approvedBy ?? createdBy,
            Details =
            [
                new StockAdjustmentDetailDto
                {
                    ItemId = itemId,
                    QuantityDifference = signed,
                    Notes = reason
                }
            ]
        };
        return await _stock.AdjustStockAsync(dto);
    }

    public async Task<bool> TransferStockAsync(CreateStockTransferDto dto, Guid tenantId, string createdBy)
    {
        if (tenantId == Guid.Empty) return false;
        dto.TenantId = tenantId;
        dto.CreatedBy = createdBy;
        if (!Guid.TryParse(createdBy, out var userId)) userId = Guid.Empty;
        var id = await _transfers.CreateAsync(dto, tenantId, userId);
        return id != Guid.Empty;
    }

    public async Task<bool> UpdateTransferStatusAsync(
        Guid transferId, Guid tenantId, string status, string? approvedBy, string updatedBy)
    {
        if (tenantId == Guid.Empty || transferId == Guid.Empty) return false;
        if (!Guid.TryParse(updatedBy, out var userId) && !Guid.TryParse(approvedBy, out userId))
            userId = Guid.Empty;

        var normalized = (status ?? string.Empty).Trim();
        if (normalized.Equals("Approved", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("InTransit", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Approve", StringComparison.OrdinalIgnoreCase))
            return await _transfers.ApproveAsync(transferId, tenantId, userId);

        if (normalized.Equals("Completed", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Complete", StringComparison.OrdinalIgnoreCase))
            return await _transfers.CompleteAsync(transferId, tenantId, userId);

        if (normalized.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
            return await _transfers.CancelAsync(transferId, tenantId, userId);

        return false;
    }

    public async Task<IEnumerable<StockTransferDto>> GetStockTransfersAsync(Guid tenantId, Guid companyId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockTransferDto>();
        return await _transfers.GetAllAsync(tenantId);
    }

    public Task<StockTransferDto?> GetStockTransferByIdAsync(Guid id, Guid tenantId)
        => _transfers.GetByIdAsync(id, tenantId);

    public async Task<IEnumerable<StockBalanceDto>> GetLowStockItemsAsync(Guid tenantId, Guid companyId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockBalanceDto>();
        var lowItems = (await _items.GetLowStockItemsAsync(tenantId, companyId)).ToList();
        if (lowItems.Count == 0) return Array.Empty<StockBalanceDto>();

        var result = new List<StockBalanceDto>();
        foreach (var item in lowItems)
        {
            var balances = await _balances.GetByItemAsync(item.Id, tenantId);
            foreach (var bal in balances)
                result.Add(MapBalance(bal));
        }
        return result;
    }

    public async Task<InventoryValuationDto> GetInventoryValuationAsync(
        Guid tenantId, Guid? warehouseId = null, DateTime? asOfDate = null)
    {
        var result = new InventoryValuationDto
        {
            TenantId = tenantId,
            WarehouseId = warehouseId,
            AsOfDate = asOfDate ?? DateTime.UtcNow.Date
        };
        if (tenantId == Guid.Empty) return result;

        IEnumerable<Domain.SCM.StockBalance> balances = warehouseId.HasValue && warehouseId != Guid.Empty
            ? await _balances.GetByWarehouseAsync(warehouseId.Value, tenantId)
            : await _balances.GetAllAsync(tenantId);

        foreach (var bal in balances.Where(b => b.CurrentQuantity != 0))
        {
            var avg = await _stock.CalculateAverageRateAsync(tenantId, new CalculateStockAverageRateRequest
            {
                ItemId = bal.ItemId,
                WarehouseId = bal.WarehouseId,
                AsOfDate = result.AsOfDate,
                UpdateItemCostPrice = false
            });
            result.Lines.Add(new InventoryValuationLineDto
            {
                ItemId = bal.ItemId,
                ItemCode = avg.ItemCode,
                ItemName = avg.ItemName ?? bal.Item?.ItemName,
                WarehouseId = bal.WarehouseId,
                QuantityOnHand = bal.CurrentQuantity,
                AverageRate = avg.AverageRate,
                TotalValue = Math.Round(bal.CurrentQuantity * avg.AverageRate, 2)
            });
        }

        result.TotalQuantity = result.Lines.Sum(l => l.QuantityOnHand);
        result.TotalValue = result.Lines.Sum(l => l.TotalValue);
        return result;
    }

    private static StockBalanceDto MapBalance(Domain.SCM.StockBalance entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        ItemId = entity.ItemId,
        ItemName = entity.Item?.ItemName ?? string.Empty,
        WarehouseId = entity.WarehouseId,
        WarehouseName = entity.Warehouse?.WarehouseName ?? string.Empty,
        QuantityOnHand = entity.CurrentQuantity,
        QuantityReserved = entity.ReservedQuantity,
        QuantityAvailable = entity.AvailableQuantity,
        LastUpdated = entity.LastUpdated,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive
    };
}
