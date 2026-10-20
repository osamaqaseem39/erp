using ERP.Application.Core.Settings;
using ERP.Application.Interfaces;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Inventory;

public class StockService : IStockService
{
    private readonly IStockBalanceRepository _repository;
    private readonly IStockLedgerRepository? _ledgers;
    private readonly ERPDbContext? _db;
    private readonly ITenantProvider? _tenant;
    private readonly ICoreSettingsService? _settings;

    public StockService(
        IStockBalanceRepository repository,
        IStockLedgerRepository? ledgers = null,
        ERPDbContext? db = null,
        ITenantProvider? tenant = null,
        ICoreSettingsService? settings = null)
    {
        _repository = repository;
        _ledgers = ledgers;
        _db = db;
        _tenant = tenant;
        _settings = settings;
    }

    private Guid CurrentTenantId =>
        _tenant?.GetTenantId() is { } tid && tid != Guid.Empty ? tid : Guid.Empty;

    public Task<StockBalanceDto?> GetByIdAsync(Guid id)
        => GetByIdAsync(id, CurrentTenantId);

    public async Task<StockBalanceDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public Task<IEnumerable<StockBalanceDto>> GetAllAsync()
        => GetAllAsync(CurrentTenantId);

    public async Task<IEnumerable<StockBalanceDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetAllAsync(tenantId)).Select(MapToDto);
    }

    public Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId)
        => GetByItemAsync(itemId, CurrentTenantId);

    public async Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByItemAsync(itemId, tenantId)).Select(MapToDto);
    }

    public Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId)
        => GetByWarehouseAsync(warehouseId, CurrentTenantId);

    public async Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByWarehouseAsync(warehouseId, tenantId)).Select(MapToDto);
    }

    public Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId)
        => GetByItemAndWarehouseAsync(itemId, warehouseId, CurrentTenantId);

    public async Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByItemAndWarehouseAsync(itemId, warehouseId, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId)
        => GetStockLevelAsync(itemId, warehouseId, CurrentTenantId);

    public async Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return 0m;
        var balances = await _repository.GetByItemAndWarehouseAllLocationsAsync(itemId, warehouseId, tenantId);
        return balances.Sum(b => b.AvailableQuantity);
    }

    public async Task<StockBalanceDto?> GetByItemWarehouseLocationAsync(
        Guid itemId, Guid warehouseId, Guid? warehouseLocationId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByItemWarehouseLocationAsync(itemId, warehouseId, warehouseLocationId, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<bool> IsBinRequiredAsync()
    {
        if (_settings == null) return false;
        var raw = await _settings.GetValueAsync(SettingsCatalog.RequireBinKey);
        return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
               || (bool.TryParse(raw, out var b) && b);
    }

    public Task<bool> AdjustStockAsync(StockAdjustmentDto dto)
        => AdjustStockAsync(dto, dto.TenantId);

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto dto, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        dto.TenantId = tenantId;
        var requireBin = await IsBinRequiredAsync();
        foreach (var detail in dto.Details)
        {
            if (requireBin && (!detail.WarehouseLocationId.HasValue || detail.WarehouseLocationId == Guid.Empty))
                return false;
            var qty = detail.QuantityDifference != 0
                ? detail.QuantityDifference
                : detail.QuantityCounted - detail.QuantityOnHand;
            var unitCost = await ResolveUnitCostAsync(detail.ItemId, tenantId, null);
            await ApplyDeltaAsync(
                tenantId, dto.WarehouseId, detail.ItemId, qty, unitCost,
                "Adjustment", dto.CreatedBy, dto.UpdatedBy, null, detail.WarehouseLocationId);
        }
        return true;
    }

    public Task<bool> ReceiveStockAsync(
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<(Guid ItemId, decimal Quantity)> lines,
        string? userId = null)
        => ReceiveStockAsync(
            tenantId,
            warehouseId,
            lines.Select(l => (l.ItemId, l.Quantity, (Guid?)null)),
            userId);

    public async Task<bool> ReceiveStockAsync(
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<(Guid ItemId, decimal Quantity, decimal UnitCost)> lines,
        string? userId = null)
    {
        if (tenantId == Guid.Empty || warehouseId == Guid.Empty) return false;
        var requireBin = await IsBinRequiredAsync();
        foreach (var (itemId, quantity, unitCost) in lines)
        {
            if (quantity <= 0) continue;
            if (requireBin) return false;
            if (!await CanReceiveTrackedAsync(tenantId, itemId, quantity)) return false;
            var cost = unitCost > 0 ? unitCost : await ResolveUnitCostAsync(itemId, tenantId, null);
            await ApplyDeltaAsync(tenantId, warehouseId, itemId, quantity, cost, "Receipt", userId, userId, null, null);
            await RecordTrackingOnReceiveAsync(tenantId, warehouseId, itemId, quantity, userId);
        }
        return true;
    }

    public async Task<bool> ReceiveStockAsync(
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<(Guid ItemId, decimal Quantity, Guid? WarehouseLocationId)> lines,
        string? userId = null)
    {
        if (tenantId == Guid.Empty || warehouseId == Guid.Empty) return false;
        var requireBin = await IsBinRequiredAsync();
        var list = lines.Where(l => l.Quantity > 0).ToList();
        foreach (var (itemId, quantity, locationId) in list)
        {
            if (requireBin && (!locationId.HasValue || locationId == Guid.Empty)) return false;
            if (!await CanReceiveTrackedAsync(tenantId, itemId, quantity)) return false;
        }

        foreach (var (itemId, quantity, locationId) in list)
        {
            var unitCost = await ResolveUnitCostAsync(itemId, tenantId, null);
            await ApplyDeltaAsync(
                tenantId, warehouseId, itemId, quantity, unitCost, "Receipt",
                userId, userId, null, NormalizeLocation(locationId));
            await RecordTrackingOnReceiveAsync(tenantId, warehouseId, itemId, quantity, userId);
        }
        return true;
    }

    public Task<bool> IssueStockAsync(
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<(Guid ItemId, decimal Quantity)> lines,
        string? userId = null)
        => IssueStockAsync(
            tenantId,
            warehouseId,
            lines.Select(l => (l.ItemId, l.Quantity, (Guid?)null)),
            userId);

    public async Task<bool> IssueStockAsync(
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<(Guid ItemId, decimal Quantity, Guid? WarehouseLocationId)> lines,
        string? userId = null)
    {
        if (tenantId == Guid.Empty || warehouseId == Guid.Empty) return false;
        var requireBin = await IsBinRequiredAsync();
        var list = lines.Where(l => l.Quantity > 0).ToList();

        foreach (var (itemId, quantity, locationId) in list)
        {
            if (requireBin && (!locationId.HasValue || locationId == Guid.Empty)) return false;
            if (!await CanIssueTrackedAsync(tenantId, warehouseId, itemId, quantity)) return false;

            if (locationId.HasValue && locationId != Guid.Empty)
            {
                var bal = await _repository.GetByItemWarehouseLocationAsync(itemId, warehouseId, locationId, tenantId);
                if (bal == null || bal.AvailableQuantity < quantity) return false;
            }
            else
            {
                var balances = await _repository.GetByItemAndWarehouseAllLocationsAsync(itemId, warehouseId, tenantId);
                if (balances.Sum(b => b.AvailableQuantity) < quantity) return false;
            }
        }

        foreach (var (itemId, quantity, locationId) in list)
        {
            var avg = await PeekAverageRateAsync(itemId, tenantId, warehouseId);
            if (locationId.HasValue && locationId != Guid.Empty)
            {
                await ApplyDeltaAsync(
                    tenantId, warehouseId, itemId, -quantity, avg, "Issue",
                    userId, userId, null, locationId);
            }
            else
            {
                var remaining = quantity;
                var balances = (await _repository.GetByItemAndWarehouseAllLocationsAsync(itemId, warehouseId, tenantId))
                    .Where(b => b.AvailableQuantity > 0)
                    .OrderBy(b => b.WarehouseLocationId == null ? 0 : 1)
                    .ThenByDescending(b => b.AvailableQuantity)
                    .ToList();
                foreach (var bal in balances)
                {
                    if (remaining <= 0) break;
                    var take = Math.Min(remaining, Math.Min(bal.AvailableQuantity, bal.CurrentQuantity));
                    await ApplyDeltaAsync(
                        tenantId, warehouseId, itemId, -take, avg, "Issue",
                        userId, userId, null, bal.WarehouseLocationId);
                    remaining -= take;
                }
                if (remaining > 0) return false;
            }

            await ConsumeTrackingOnIssueAsync(tenantId, warehouseId, itemId, quantity, userId);
        }
        return true;
    }

    public async Task<bool> IssueStockAnyWarehouseAsync(
        Guid tenantId,
        IEnumerable<(Guid ItemId, decimal Quantity)> lines,
        string? userId = null)
    {
        if (tenantId == Guid.Empty) return false;
        if (await IsBinRequiredAsync()) return false;

        foreach (var (itemId, quantity) in lines.Where(l => l.Quantity > 0))
        {
            var remaining = quantity;
            var balances = (await _repository.GetByItemAsync(itemId, tenantId))
                .Where(b => b.AvailableQuantity > 0)
                .OrderByDescending(b => b.AvailableQuantity)
                .ToList();

            if (balances.Sum(b => b.AvailableQuantity) < quantity) return false;

            foreach (var bal in balances)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, Math.Min(bal.AvailableQuantity, bal.CurrentQuantity));
                var avg = await PeekAverageRateAsync(itemId, tenantId, bal.WarehouseId);
                await ApplyDeltaAsync(
                    tenantId, bal.WarehouseId, itemId, -take, avg, "Issue",
                    userId, userId, null, bal.WarehouseLocationId);
                remaining -= take;
            }
        }
        return true;
    }

    public async Task<bool> ReserveAsync(
        Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId = null)
    {
        if (tenantId == Guid.Empty || warehouseId == Guid.Empty || itemId == Guid.Empty || quantity <= 0)
            return false;

        var balances = (await _repository.GetByItemAndWarehouseAllLocationsAsync(itemId, warehouseId, tenantId)).ToList();
        if (balances.Sum(b => b.AvailableQuantity) < quantity) return false;

        var bucket = await _repository.GetByItemWarehouseLocationAsync(itemId, warehouseId, null, tenantId);
        if (bucket == null)
        {
            bucket = new StockBalance
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ItemId = itemId,
                WarehouseId = warehouseId,
                WarehouseLocationId = null,
                CurrentQuantity = 0,
                ReservedQuantity = quantity,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await _repository.CreateAsync(bucket);
            return true;
        }

        bucket.ReservedQuantity += quantity;
        bucket.LastUpdated = DateTime.UtcNow;
        bucket.UpdatedBy = userId;
        await _repository.UpdateAsync(bucket);
        return true;
    }

    public async Task<bool> UnreserveAsync(
        Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId = null)
    {
        if (tenantId == Guid.Empty || warehouseId == Guid.Empty || itemId == Guid.Empty || quantity <= 0)
            return false;

        var bucket = await _repository.GetByItemWarehouseLocationAsync(itemId, warehouseId, null, tenantId);
        if (bucket == null) return true;

        bucket.ReservedQuantity = Math.Max(0, bucket.ReservedQuantity - quantity);
        bucket.LastUpdated = DateTime.UtcNow;
        bucket.UpdatedBy = userId;
        await _repository.UpdateAsync(bucket);
        return true;
    }

    public async Task<bool> TransferStockAsync(StockTransferDto dto)
    {
        if (dto.TenantId == Guid.Empty) return false;
        foreach (var detail in dto.Details)
        {
            var from = await _repository.GetByItemAndWarehouseAsync(detail.ItemId, dto.FromWarehouseId, dto.TenantId);
            if (from == null || from.AvailableQuantity < detail.Quantity)
                return false;

            var avg = await PeekAverageRateAsync(detail.ItemId, dto.TenantId, dto.FromWarehouseId);
            await ApplyDeltaAsync(
                dto.TenantId, dto.FromWarehouseId, detail.ItemId, -detail.Quantity, avg,
                "Transfer", dto.CreatedBy, dto.UpdatedBy,
                reference: $"To:{dto.ToWarehouseId}");
            await ApplyDeltaAsync(
                dto.TenantId, dto.ToWarehouseId, detail.ItemId, detail.Quantity, avg,
                "Transfer", dto.CreatedBy, dto.UpdatedBy,
                reference: $"From:{dto.FromWarehouseId}");
        }
        return true;
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(
        Guid itemId, Guid tenantId, Guid? warehouseId = null, DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty) return [];
        if (_ledgers != null)
        {
            var rows = await _ledgers.GetByItemAsync(itemId, tenantId, warehouseId, toDate);
            return rows.Select(MapLedger).ToList();
        }

        if (_db == null) return [];
        var q = _db.StockLedgers.AsNoTracking()
            .Include(l => l.Item)
            .Include(l => l.Warehouse)
            .Where(l => l.ItemId == itemId && l.TenantId == tenantId);
        if (warehouseId.HasValue && warehouseId != Guid.Empty)
            q = q.Where(l => l.WarehouseId == warehouseId);
        // Inclusive calendar day: clients often pass date-only (midnight), which would
        // otherwise exclude same-day ledger rows stamped with a full UTC timestamp.
        if (toDate.HasValue)
        {
            var endInclusive = toDate.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(l => l.TransactionDate <= endInclusive);
        }
        var list = await q.OrderBy(l => l.TransactionDate).ThenBy(l => l.CreatedAt).ToListAsync();
        return list.Select(MapLedger).ToList();
    }

    public async Task<StockAverageRateDto> CalculateAverageRateAsync(
        Guid tenantId,
        CalculateStockAverageRateRequest request)
    {
        if (tenantId == Guid.Empty)
            return new StockAverageRateDto { ItemId = request.ItemId, WarehouseId = request.WarehouseId, AsOfDate = request.AsOfDate };

        var item = _db == null
            ? null
            : await _db.Items.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.TenantId == tenantId);

        var ledgers = (await GetStockLedgerAsync(
            request.ItemId, tenantId, request.WarehouseId, request.AsOfDate)).ToList();

        decimal qty = 0m;
        decimal value = 0m;
        foreach (var row in ledgers.OrderBy(l => l.TransactionDate))
        {
            if (row.Quantity > 0)
            {
                var cost = row.UnitCost;
                value = Math.Round(value + row.Quantity * cost, 4);
                qty = Math.Round(qty + row.Quantity, 4);
            }
            else if (row.Quantity < 0)
            {
                var outQty = Math.Abs(row.Quantity);
                var avg = qty > 0 ? value / qty : 0m;
                value = Math.Round(Math.Max(0, value - outQty * avg), 4);
                qty = Math.Round(Math.Max(0, qty - outQty), 4);
            }
        }

        // Prefer balance on-hand when ledger history is incomplete
        decimal onHand;
        if (request.WarehouseId.HasValue && request.WarehouseId != Guid.Empty)
        {
            var bal = await _repository.GetByItemAndWarehouseAsync(
                request.ItemId, request.WarehouseId.Value, tenantId);
            onHand = bal?.CurrentQuantity ?? qty;
        }
        else
        {
            onHand = (await _repository.GetByItemAsync(request.ItemId, tenantId))
                .Sum(b => b.CurrentQuantity);
            if (onHand == 0 && qty != 0) onHand = qty;
        }

        var average = qty > 0 ? Math.Round(value / qty, 4) : Math.Round(item?.CostPrice ?? 0m, 4);
        if (qty <= 0 && onHand > 0 && item != null)
            average = Math.Round(item.CostPrice, 4);

        var previous = item?.CostPrice ?? 0m;
        var updated = false;
        if (request.UpdateItemCostPrice && _db != null && item != null && average > 0)
        {
            var tracked = await _db.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId);
            if (tracked != null)
            {
                tracked.CostPrice = average;
                tracked.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                updated = true;
            }
        }

        return new StockAverageRateDto
        {
            ItemId = request.ItemId,
            ItemCode = item?.ItemCode,
            ItemName = item?.ItemName,
            WarehouseId = request.WarehouseId,
            AsOfDate = request.AsOfDate,
            QuantityOnHand = Math.Round(onHand, 4),
            TotalValue = Math.Round(average * onHand, 2),
            AverageRate = average,
            PreviousCostPrice = previous,
            CostPriceUpdated = updated,
            LedgerEntryCount = ledgers.Count
        };
    }

    public Task<Guid> CreateAsync(StockBalanceDto dto)
        => CreateAsync(dto, dto.TenantId != Guid.Empty ? dto.TenantId : CurrentTenantId);

    public async Task<Guid> CreateAsync(StockBalanceDto dto, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");
        dto.TenantId = tenantId;
        var entity = MapToEntity(dto);
        var created = await _repository.CreateAsync(entity);
        return created.Id;
    }

    public Task<bool> UpdateAsync(StockBalanceDto dto)
        => UpdateAsync(dto, dto.TenantId != Guid.Empty ? dto.TenantId : CurrentTenantId);

    public async Task<bool> UpdateAsync(StockBalanceDto dto, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public Task<bool> DeleteAsync(Guid id)
        => DeleteAsync(id, CurrentTenantId);

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    private async Task ApplyDeltaAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid itemId,
        decimal delta,
        decimal unitCost,
        string transactionType,
        string? createdBy,
        string? updatedBy,
        string? reference = null,
        Guid? warehouseLocationId = null)
    {
        if (delta == 0) return;

        var locationId = NormalizeLocation(warehouseLocationId);
        var entity = await _repository.GetByItemWarehouseLocationAsync(itemId, warehouseId, locationId, tenantId);
        if (entity == null)
        {
            if (delta < 0) return;
            entity = new StockBalance
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ItemId = itemId,
                WarehouseId = warehouseId,
                WarehouseLocationId = locationId,
                CurrentQuantity = delta,
                ReservedQuantity = 0,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            await _repository.CreateAsync(entity);
        }
        else
        {
            entity.CurrentQuantity += delta;
            entity.LastUpdated = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy ?? createdBy;
            await _repository.UpdateAsync(entity);
        }

        await WriteLedgerAsync(
            tenantId, warehouseId, itemId, delta, unitCost, transactionType,
            entity.CurrentQuantity, createdBy, reference, locationId);
    }

    private async Task WriteLedgerAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid itemId,
        decimal delta,
        decimal unitCost,
        string transactionType,
        decimal balanceQty,
        string? createdBy,
        string? reference,
        Guid? warehouseLocationId = null)
    {
        var qtyIn = delta > 0 ? delta : 0m;
        var qtyOut = delta < 0 ? Math.Abs(delta) : 0m;
        var cost = Math.Round(unitCost, 4);
        var row = new StockLedger
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ItemId = itemId,
            WarehouseId = warehouseId,
            WarehouseLocationId = warehouseLocationId,
            TransactionType = transactionType,
            TransactionDate = DateTime.UtcNow,
            QuantityIn = qtyIn,
            QuantityOut = qtyOut,
            BalanceQty = balanceQty,
            UnitPrice = cost,
            TotalValue = Math.Round((qtyIn + qtyOut) * cost, 2),
            ReferenceDocType = reference,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        if (_ledgers != null)
        {
            await _ledgers.AddAsync(row);
            return;
        }

        if (_db == null) return;
        _db.StockLedgers.Add(row);
        await _db.SaveChangesAsync();
    }

    private static Guid? NormalizeLocation(Guid? warehouseLocationId)
        => warehouseLocationId is null || warehouseLocationId == Guid.Empty ? null : warehouseLocationId;

    private async Task<decimal> ResolveUnitCostAsync(Guid itemId, Guid tenantId, decimal? overrideCost)
    {
        if (overrideCost.HasValue && overrideCost.Value > 0) return overrideCost.Value;
        if (_db == null) return 0m;
        if (tenantId == Guid.Empty) return 0m;
        var item = await _db.Items.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TenantId == tenantId);
        return item?.CostPrice ?? 0m;
    }

    private async Task<Item?> LoadItemAsync(Guid itemId, Guid tenantId)
    {
        if (_db == null || tenantId == Guid.Empty) return null;
        return await _db.Items.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TenantId == tenantId);
    }

    private async Task<bool> CanReceiveTrackedAsync(Guid tenantId, Guid itemId, decimal quantity)
    {
        var item = await LoadItemAsync(itemId, tenantId);
        if (item == null) return true;
        if (item.IsSerialized && quantity != Math.Floor(quantity))
            return false;
        return true;
    }

    private async Task<bool> CanIssueTrackedAsync(Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity)
    {
        if (_db == null) return true;
        var item = await LoadItemAsync(itemId, tenantId);
        if (item == null) return true;

        if (item.IsSerialized)
        {
            if (quantity != Math.Floor(quantity)) return false;
            var need = (int)quantity;
            var available = await _db.SerialNumbers.CountAsync(s =>
                s.TenantId == tenantId && s.ItemId == itemId && s.IsActive && s.Status == "Available");
            return available >= need;
        }

        if (item.IsBatchTracked)
        {
            var tracked = await _db.BatchSerialTrackings
                .Where(t => t.TenantId == tenantId && t.ItemId == itemId && t.WarehouseId == warehouseId
                            && t.IsActive && t.Status == "Available" && t.Quantity > 0
                            && t.BatchNo != null)
                .SumAsync(t => (decimal?)t.Quantity) ?? 0m;
            if (tracked > 0) return tracked >= quantity;

            var onHand = await _db.BatchNumbers
                .Where(b => b.TenantId == tenantId && b.ItemId == itemId && b.IsActive && b.Quantity > 0)
                .SumAsync(b => (decimal?)b.Quantity) ?? 0m;
            return onHand >= quantity;
        }

        return true;
    }

    private async Task RecordTrackingOnReceiveAsync(
        Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId)
    {
        if (_db == null || quantity <= 0) return;
        var item = await LoadItemAsync(itemId, tenantId);
        if (item == null) return;

        var stamp = DateTime.UtcNow;
        if (item.IsBatchTracked)
        {
            var batchCode = $"B-{stamp:yyyyMMdd}-{Guid.NewGuid():N}"[..20];
            _db.BatchNumbers.Add(new BatchNumber
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ItemId = itemId,
                BatchNumberValue = batchCode,
                ManufacturingDate = stamp.Date,
                Quantity = quantity,
                IsActive = true,
                CreatedAt = stamp,
                CreatedBy = userId
            });
            _db.BatchSerialTrackings.Add(new BatchSerialTracking
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ItemId = itemId,
                WarehouseId = warehouseId,
                BatchNo = batchCode,
                ManufactureDate = stamp.Date,
                Quantity = quantity,
                Status = "Available",
                IsActive = true,
                CreatedAt = stamp,
                CreatedBy = userId
            });
        }

        if (item.IsSerialized)
        {
            var count = (int)quantity;
            for (var i = 0; i < count; i++)
            {
                var serial = $"S-{stamp:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..28];
                _db.SerialNumbers.Add(new SerialNumber
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ItemId = itemId,
                    SerialNumberValue = serial,
                    Status = "Available",
                    Location = warehouseId.ToString("N"),
                    IsActive = true,
                    CreatedAt = stamp,
                    CreatedBy = userId
                });
                _db.BatchSerialTrackings.Add(new BatchSerialTracking
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ItemId = itemId,
                    WarehouseId = warehouseId,
                    SerialNo = serial,
                    Quantity = 1m,
                    Status = "Available",
                    IsActive = true,
                    CreatedAt = stamp,
                    CreatedBy = userId
                });
            }
        }

        if (item.IsBatchTracked || item.IsSerialized)
            await _db.SaveChangesAsync();
    }

    private async Task ConsumeTrackingOnIssueAsync(
        Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId)
    {
        if (_db == null || quantity <= 0) return;
        var item = await LoadItemAsync(itemId, tenantId);
        if (item == null) return;
        var stamp = DateTime.UtcNow;

        if (item.IsSerialized)
        {
            var need = (int)quantity;
            var serials = await _db.SerialNumbers
                .Where(s => s.TenantId == tenantId && s.ItemId == itemId && s.IsActive && s.Status == "Available")
                .OrderBy(s => s.CreatedAt)
                .Take(need)
                .ToListAsync();
            foreach (var s in serials)
            {
                s.Status = "Issued";
                s.UpdatedAt = stamp;
                s.UpdatedBy = userId;
                var tracks = await _db.BatchSerialTrackings
                    .Where(t => t.TenantId == tenantId && t.ItemId == itemId && t.SerialNo == s.SerialNumberValue
                                && t.Status == "Available" && t.IsActive)
                    .ToListAsync();
                foreach (var t in tracks)
                {
                    t.Status = "Issued";
                    t.Quantity = 0;
                    t.UpdatedAt = stamp;
                    t.UpdatedBy = userId;
                }
            }
        }

        if (item.IsBatchTracked)
        {
            var remaining = quantity;
            var tracks = await _db.BatchSerialTrackings
                .Where(t => t.TenantId == tenantId && t.ItemId == itemId && t.WarehouseId == warehouseId
                            && t.IsActive && t.Status == "Available" && t.Quantity > 0 && t.BatchNo != null)
                .OrderBy(t => t.ExpiryDate ?? DateTime.MaxValue)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            foreach (var t in tracks)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, t.Quantity);
                t.Quantity -= take;
                if (t.Quantity <= 0)
                {
                    t.Quantity = 0;
                    t.Status = "Issued";
                }
                t.UpdatedAt = stamp;
                t.UpdatedBy = userId;

                var batch = await _db.BatchNumbers
                    .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.ItemId == itemId
                                              && b.BatchNumberValue == t.BatchNo && b.IsActive);
                if (batch != null)
                {
                    batch.Quantity = Math.Max(0, batch.Quantity - take);
                    if (batch.Quantity <= 0) batch.IsActive = false;
                    batch.UpdatedAt = stamp;
                    batch.UpdatedBy = userId;
                }

                remaining -= take;
            }

            if (remaining > 0)
            {
                var batches = await _db.BatchNumbers
                    .Where(b => b.TenantId == tenantId && b.ItemId == itemId && b.IsActive && b.Quantity > 0)
                    .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue)
                    .ThenBy(b => b.CreatedAt)
                    .ToListAsync();
                foreach (var batch in batches)
                {
                    if (remaining <= 0) break;
                    var take = Math.Min(remaining, batch.Quantity);
                    batch.Quantity -= take;
                    if (batch.Quantity <= 0) batch.IsActive = false;
                    batch.UpdatedAt = stamp;
                    batch.UpdatedBy = userId;
                    remaining -= take;
                }
            }
        }

        if (item.IsBatchTracked || item.IsSerialized)
            await _db.SaveChangesAsync();
    }

    private async Task<decimal> PeekAverageRateAsync(Guid itemId, Guid tenantId, Guid? warehouseId)
    {
        var result = await CalculateAverageRateAsync(tenantId, new CalculateStockAverageRateRequest
        {
            ItemId = itemId,
            WarehouseId = warehouseId,
            UpdateItemCostPrice = false
        });
        return result.AverageRate;
    }

    private static StockBalanceDto MapToDto(StockBalance entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        ItemId = entity.ItemId,
        ItemName = entity.Item?.ItemName ?? string.Empty,
        WarehouseId = entity.WarehouseId,
        WarehouseLocationId = entity.WarehouseLocationId,
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

    private static StockLedgerDto MapLedger(StockLedger entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        ItemId = entity.ItemId,
        ItemName = entity.Item?.ItemName ?? string.Empty,
        WarehouseId = entity.WarehouseId,
        WarehouseLocationId = entity.WarehouseLocationId,
        WarehouseName = entity.Warehouse?.WarehouseName ?? string.Empty,
        TransactionType = entity.TransactionType,
        Quantity = entity.QuantityIn - entity.QuantityOut,
        UnitCost = entity.UnitPrice,
        TotalCost = entity.TotalValue,
        Reference = entity.ReferenceDocType ?? string.Empty,
        TransactionDate = entity.TransactionDate,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy
    };

    private static StockLedgerDto MapLedger(StockLedgerDto dto) => dto;

    private static StockBalance MapToEntity(StockBalanceDto dto) => new()
    {
        Id = dto.Id,
        TenantId = dto.TenantId,
        ItemId = dto.ItemId,
        WarehouseId = dto.WarehouseId,
        WarehouseLocationId = dto.WarehouseLocationId,
        CurrentQuantity = dto.QuantityOnHand,
        ReservedQuantity = dto.QuantityReserved,
        LastUpdated = dto.LastUpdated,
        CreatedBy = dto.CreatedBy
    };

    private static void MapToEntity(StockBalanceDto dto, StockBalance entity)
    {
        entity.ItemId = dto.ItemId;
        entity.WarehouseId = dto.WarehouseId;
        entity.WarehouseLocationId = dto.WarehouseLocationId;
        entity.CurrentQuantity = dto.QuantityOnHand;
        entity.ReservedQuantity = dto.QuantityReserved;
        entity.LastUpdated = dto.LastUpdated;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}
