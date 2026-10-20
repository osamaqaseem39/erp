using ERP.Application.Interfaces;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Manufacturing;

public class ProductionOrderService : IProductionOrderService
{
    private static readonly HashSet<string> StartableStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "Draft", "Planned", "Released" };

    private static readonly HashSet<string> CompletableStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "InProgress", "QualityCheck" };

    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionOrderLineRepository _productionOrderLineRepository;
    private readonly IProductionRunRepository _productionRunRepository;
    private readonly IWorkCenterRepository _workCenterRepository;
    private readonly IBillOfMaterialRepository _boms;
    private readonly ERPDbContext _db;
    private readonly IStockService? _stock;

    public ProductionOrderService(
        IProductionOrderRepository productionOrderRepository,
        IProductionOrderLineRepository productionOrderLineRepository,
        IProductionRunRepository productionRunRepository,
        IWorkCenterRepository workCenterRepository,
        IBillOfMaterialRepository boms,
        ERPDbContext db,
        IStockService? stock = null)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionOrderLineRepository = productionOrderLineRepository;
        _productionRunRepository = productionRunRepository;
        _workCenterRepository = workCenterRepository;
        _boms = boms;
        _db = db;
        _stock = stock;
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id, tenantId);
        if (productionOrder == null) return null;
        await LoadChildrenAsync(productionOrder, tenantId);
        return MapToDto(productionOrder);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetAllAsync(Guid tenantId, string? status = null, Guid? workCenterId = null)
    {
        IEnumerable<ProductionOrder> productionOrders = await _productionOrderRepository.GetAllAsync(tenantId);

        if (status != null)
            productionOrders = productionOrders.Where(po => po.Status == status);
        if (workCenterId.HasValue)
            productionOrders = productionOrders.Where(po => po.WorkCenterId == workCenterId.Value);

        return productionOrders.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var productionOrders = await _productionOrderRepository.GetByStatusAsync(status, tenantId);
        return productionOrders.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetByWorkCenterAsync(Guid workCenterId, Guid tenantId)
    {
        var productionOrders = await _productionOrderRepository.GetByWorkCenterAsync(workCenterId, tenantId);
        return productionOrders.Select(MapToDto);
    }

    public async Task<ProductionOrderDto> CreateAsync(CreateProductionOrderDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        var bomId = dto.BomId == Guid.Empty ? null : dto.BomId;
        var routingId = dto.RoutingId == Guid.Empty ? null : dto.RoutingId;
        var itemId = dto.ItemId;

        if (bomId.HasValue)
        {
            var bom = await _boms.GetByIdAsync(bomId.Value, tenantId)
                ?? throw new ArgumentException("BOM not found.");
            if (!string.Equals(bom.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("BOM must be Active.");
            itemId ??= bom.FinishedGoodItemId;
        }
        else if (itemId.HasValue)
        {
            var defaultBom = await _boms.GetActiveByFinishedGoodAsync(itemId.Value, tenantId);
            if (defaultBom != null) bomId = defaultBom.Id;
        }

        if (!routingId.HasValue && itemId.HasValue)
        {
            var defaultRouting = await _db.Routings.AsNoTracking()
                .Where(r => r.TenantId == tenantId && r.IsActive && r.Status == "Active"
                    && r.ItemId == itemId)
                .OrderByDescending(r => r.IsDefault)
                .ThenByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .FirstOrDefaultAsync();
            if (defaultRouting != null) routingId = defaultRouting.Id;
        }

        var status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status.Trim();
        if (dto.SalesOrderLineId.HasValue && dto.SalesOrderLineId != Guid.Empty)
            status = "Released";

        var productionOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = dto.OrderNumber ?? Guid.NewGuid().ToString("N")[..8],
            ProductionOrderNumber = dto.ProductionOrderNumber ?? string.Empty,
            ItemId = itemId,
            BomId = bomId,
            RoutingId = routingId,
            Quantity = dto.Quantity ?? 0m,
            WorkCenterId = dto.WorkCenterId,
            SalesOrderId = dto.SalesOrderId == Guid.Empty ? null : dto.SalesOrderId,
            SalesOrderLineId = dto.SalesOrderLineId == Guid.Empty ? null : dto.SalesOrderLineId,
            WarehouseId = dto.WarehouseId == Guid.Empty ? null : dto.WarehouseId,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            Status = status,
            Priority = dto.Priority ?? "Medium",
            Notes = dto.Notes,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _productionOrderRepository.CreateAsync(productionOrder);
        await ExplodeBomLinesAsync(productionOrder, tenantId, userId);

        var created = await _productionOrderRepository.GetByIdAsync(createdId, tenantId);
        await LoadChildrenAsync(created!, tenantId);
        return MapToDto(created!);
    }

    public async Task<ProductionOrderDto> UpdateAsync(UpdateProductionOrderDto dto, Guid tenantId, Guid userId)
    {
        var productionOrder = await RequireTenantOrderAsync(dto.Id, tenantId);
        if (productionOrder.Status is "Completed" or "Cancelled")
            throw new InvalidOperationException("Cannot update completed or cancelled production orders.");

        productionOrder.OrderNumber = dto.OrderNumber ?? productionOrder.OrderNumber;
        productionOrder.ItemId = dto.ItemId ?? productionOrder.ItemId;
        productionOrder.BomId = dto.BomId ?? productionOrder.BomId;
        productionOrder.RoutingId = dto.RoutingId ?? productionOrder.RoutingId;
        productionOrder.Quantity = dto.Quantity ?? productionOrder.Quantity;
        productionOrder.WorkCenterId = dto.WorkCenterId ?? productionOrder.WorkCenterId;
        productionOrder.WarehouseId = dto.WarehouseId ?? productionOrder.WarehouseId;
        productionOrder.PlannedStartDate = dto.PlannedStartDate ?? productionOrder.PlannedStartDate;
        productionOrder.PlannedEndDate = dto.PlannedEndDate ?? productionOrder.PlannedEndDate;
        productionOrder.Priority = dto.Priority ?? productionOrder.Priority;
        productionOrder.Notes = dto.Notes ?? productionOrder.Notes;
        if (!string.IsNullOrWhiteSpace(dto.Status)
            && StartableStatuses.Contains(dto.Status)
            && StartableStatuses.Contains(productionOrder.Status))
            productionOrder.Status = dto.Status;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        return MapToDto(productionOrder);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id, tenantId);
        if (productionOrder == null) return false;
        if (productionOrder.Status is not "Draft")
            throw new InvalidOperationException("Only Draft production orders can be deleted.");
        return await _productionOrderRepository.DeleteAsync(id, tenantId);
    }

    public async Task<ProductionOrderDto> StartAsync(Guid id, Guid tenantId, Guid userId)
    {
        var productionOrder = await RequireTenantOrderAsync(id, tenantId);

        if (!StartableStatuses.Contains(productionOrder.Status))
            throw new InvalidOperationException("Only Draft, Planned, or Released production orders can be started.");

        productionOrder.Status = "InProgress";
        productionOrder.ActualStartDate = DateTime.UtcNow;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        await EnsureOperationsFromRoutingAsync(productionOrder, tenantId, userId);

        var firstOp = await _db.ProductionOrderOperations
            .Where(o => o.ProductionOrderId == productionOrder.Id && o.TenantId == tenantId && o.IsActive)
            .OrderBy(o => o.StepNumber)
            .FirstOrDefaultAsync();

        var run = new ProductionRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductionOrderId = productionOrder.Id,
            RunNumber = $"RUN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            StartTime = DateTime.UtcNow,
            Status = "InProgress",
            WorkCenterId = firstOp?.WorkCenterId ?? productionOrder.WorkCenterId,
            ProductionOrderOperationId = firstOp?.Id,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _productionRunRepository.CreateAsync(run);

        if (firstOp != null && firstOp.Status == "Pending")
        {
            firstOp.Status = "InProgress";
            firstOp.ActualStart = DateTime.UtcNow;
            firstOp.UpdatedAt = DateTime.UtcNow;
            firstOp.UpdatedBy = userId.ToString();
            await _db.SaveChangesAsync();
        }

        await LoadChildrenAsync(productionOrder, tenantId);
        return MapToDto(productionOrder);
    }

    public async Task<ProductionOrderDto> CompleteAsync(Guid id, CompleteProductionOrderDto dto, Guid tenantId, Guid userId)
    {
        var productionOrder = await RequireTenantOrderAsync(id, tenantId);

        if (!CompletableStatuses.Contains(productionOrder.Status))
            throw new InvalidOperationException("Only InProgress or QualityCheck production orders can be completed.");

        if (productionOrder.RoutingId.HasValue)
        {
            var ops = await _db.ProductionOrderOperations
                .Where(o => o.ProductionOrderId == id && o.TenantId == tenantId && o.IsActive)
                .ToListAsync();
            if (ops.Count > 0 && ops.Any(o =>
                    !string.Equals(o.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(o.Status, "Skipped", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("All routing operations must be Completed or Skipped before completing the order.");

            var passedQc = await _db.QualityChecks.AsNoTracking()
                .AnyAsync(q => q.ProductionOrderId == id && q.TenantId == tenantId && q.IsActive
                    && q.Status == "Passed");
            if (!passedQc)
                throw new InvalidOperationException("A Passed quality check is required when the order has a routing.");
        }

        var qty = dto.ActualQuantity > 0 ? dto.ActualQuantity : productionOrder.Quantity;
        if (qty <= 0)
            throw new ArgumentException("Completion quantity must be greater than zero.");

        var warehouseId = dto.WarehouseId ?? productionOrder.WarehouseId;
        if (warehouseId.HasValue && warehouseId != Guid.Empty && productionOrder.WarehouseId == null)
            productionOrder.WarehouseId = warehouseId;

        await ApplyBomStockAsync(productionOrder, qty, warehouseId, tenantId, userId);
        await MarkMaterialLinesConsumedAsync(productionOrder, qty, tenantId, userId);

        productionOrder.Status = "Completed";
        productionOrder.CompletedQuantity = qty;
        productionOrder.ActualEndDate = dto.CompletionDate == default ? DateTime.UtcNow : dto.CompletionDate.ToUniversalTime();
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            productionOrder.Notes = string.IsNullOrWhiteSpace(productionOrder.Notes)
                ? dto.Notes
                : $"{productionOrder.Notes}\n{dto.Notes}";
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        await LoadChildrenAsync(productionOrder, tenantId);
        return MapToDto(productionOrder);
    }

    public async Task<ProductionOrderDto> CancelAsync(Guid id, string reason, Guid tenantId, Guid userId)
    {
        var productionOrder = await RequireTenantOrderAsync(id, tenantId);

        if (productionOrder.Status is "Completed" or "Cancelled")
            throw new InvalidOperationException("Cannot cancel completed or already cancelled production orders.");

        productionOrder.Status = "Cancelled";
        productionOrder.CancellationReason = reason ?? string.Empty;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        return MapToDto(productionOrder);
    }

    public async Task<IReadOnlyList<ProductionOrderOperationDto>> ListOperationsAsync(Guid productionOrderId, Guid tenantId)
    {
        var order = await _productionOrderRepository.GetByIdAsync(productionOrderId, tenantId);
        if (order == null) return Array.Empty<ProductionOrderOperationDto>();
        var ops = await _db.ProductionOrderOperations.AsNoTracking()
            .Where(o => o.ProductionOrderId == productionOrderId && o.TenantId == tenantId && o.IsActive)
            .OrderBy(o => o.StepNumber)
            .ToListAsync();
        return ops.Select(MapOp).ToList();
    }

    public async Task<ProductionOrderOperationDto> CompleteOperationAsync(
        Guid productionOrderId, Guid operationId, CompleteProductionOrderOperationDto dto, Guid tenantId, Guid userId)
    {
        var order = await RequireTenantOrderAsync(productionOrderId, tenantId);
        if (!string.Equals(order.Status, "InProgress", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(order.Status, "QualityCheck", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Operations can only be completed while the order is InProgress or QualityCheck.");

        var op = await _db.ProductionOrderOperations
            .FirstOrDefaultAsync(o => o.Id == operationId && o.ProductionOrderId == productionOrderId
                && o.TenantId == tenantId && o.IsActive)
            ?? throw new KeyNotFoundException("Operation not found.");

        if (string.Equals(op.Status, "Completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(op.Status, "Skipped", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Operation is already {op.Status}.");

        op.Status = "Completed";
        op.ActualStart ??= DateTime.UtcNow;
        op.ActualEnd = DateTime.UtcNow;
        op.ActualTimeMinutes = dto.ActualTimeMinutes
            ?? (decimal)(op.ActualEnd.Value - op.ActualStart.Value).TotalMinutes;
        if (!string.IsNullOrWhiteSpace(dto.Notes)) op.Notes = dto.Notes;
        op.UpdatedAt = DateTime.UtcNow;
        op.UpdatedBy = userId.ToString();
        await _db.SaveChangesAsync();

        var remaining = await _db.ProductionOrderOperations
            .AnyAsync(o => o.ProductionOrderId == productionOrderId && o.TenantId == tenantId && o.IsActive
                && o.Status != "Completed" && o.Status != "Skipped");
        if (!remaining && order.RoutingId.HasValue
            && string.Equals(order.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
        {
            order.Status = "QualityCheck";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId.ToString();
            await _productionOrderRepository.UpdateAsync(order);
        }

        return MapOp(op);
    }

    public async Task<WorkCenterCapacityWarnDto> GetCapacityWarnAsync(Guid workCenterId, Guid tenantId, DateTime? weekStartUtc = null)
    {
        var wc = await _workCenterRepository.GetByIdAsync(workCenterId, tenantId)
            ?? throw new KeyNotFoundException("Work center not found.");

        var start = weekStartUtc?.Date ?? StartOfWeekUtc(DateTime.UtcNow);
        var end = start.AddDays(7);
        var capacity = wc.WeeklyAvailableHours > 0 ? wc.WeeklyAvailableHours : wc.Capacity;

        var openOrders = await _db.ProductionOrders.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive
                && p.WorkCenterId == workCenterId
                && p.Status != "Completed" && p.Status != "Cancelled"
                && ((p.PlannedStartDate == null && p.PlannedEndDate == null)
                    || (p.PlannedStartDate != null && p.PlannedStartDate < end
                        && (p.PlannedEndDate == null || p.PlannedEndDate >= start))))
            .ToListAsync();

        decimal loadMinutes = 0;
        foreach (var order in openOrders)
        {
            if (order.RoutingId.HasValue)
            {
                var steps = await _db.RoutingSteps.AsNoTracking()
                    .Where(s => s.RoutingId == order.RoutingId && s.IsActive
                        && (s.WorkCenterId == null || s.WorkCenterId == workCenterId))
                    .ToListAsync();
                loadMinutes += steps.Sum(s => s.StandardTimeMinutes) * order.Quantity;
            }
            else
            {
                loadMinutes += 60m * order.Quantity; // fallback 1 hour per unit
            }
        }

        var loadHours = Math.Round(loadMinutes / 60m, 2);
        return new WorkCenterCapacityWarnDto
        {
            WorkCenterId = wc.Id,
            WorkCenterCode = wc.WorkCenterCode,
            WorkCenterName = wc.WorkCenterName,
            CapacityHours = capacity,
            LoadHours = loadHours,
            Overloaded = loadHours > capacity,
            WeekStartUtc = start
        };
    }

    private async Task ExplodeBomLinesAsync(ProductionOrder order, Guid tenantId, Guid userId)
    {
        if (!order.BomId.HasValue) return;
        var bom = await _boms.GetByIdAsync(order.BomId.Value, tenantId);
        if (bom == null) return;

        var existing = (await _productionOrderLineRepository.GetByProductionOrderAsync(order.Id, tenantId)).ToList();
        if (existing.Count > 0) return;

        foreach (var line in bom.Lines.Where(l => l.IsActive).OrderBy(l => l.LineNumber))
        {
            var need = line.QuantityPer * order.Quantity;
            if (line.ScrapPercent > 0)
                need *= 1m + (line.ScrapPercent / 100m);
            await _productionOrderLineRepository.CreateAsync(new ProductionOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductionOrderId = order.Id,
                ItemId = line.ComponentItemId,
                Quantity = Math.Round(need, 4),
                ConsumedQuantity = 0,
                LineType = "Material",
                Notes = string.IsNullOrWhiteSpace(line.AlternateGroup)
                    ? line.Notes
                    : $"AltGroup:{line.AlternateGroup};{line.Notes}",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                IsActive = true
            });
        }
    }

    private async Task EnsureOperationsFromRoutingAsync(ProductionOrder order, Guid tenantId, Guid userId)
    {
        if (!order.RoutingId.HasValue) return;
        var existing = await _db.ProductionOrderOperations
            .AnyAsync(o => o.ProductionOrderId == order.Id && o.TenantId == tenantId && o.IsActive);
        if (existing) return;

        var steps = await _db.RoutingSteps
            .Where(s => s.RoutingId == order.RoutingId && s.IsActive)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();
        foreach (var step in steps)
        {
            _db.ProductionOrderOperations.Add(new ProductionOrderOperation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductionOrderId = order.Id,
                RoutingStepId = step.Id,
                StepNumber = step.StepNumber,
                WorkCenterId = step.WorkCenterId ?? order.WorkCenterId,
                OperationName = step.OperationName,
                StandardTimeMinutes = step.StandardTimeMinutes,
                Status = "Pending",
                Notes = step.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                IsActive = true
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task ApplyBomStockAsync(
        ProductionOrder order, decimal qty, Guid? warehouseId, Guid tenantId, Guid userId)
    {
        BillOfMaterial? bom = null;
        if (order.BomId.HasValue && order.BomId != Guid.Empty)
            bom = await _boms.GetByIdAsync(order.BomId.Value, tenantId);
        else if (order.ItemId.HasValue)
            bom = await _boms.GetActiveByFinishedGoodAsync(order.ItemId.Value, tenantId);

        if (bom == null) return;

        if (_stock == null)
            throw new InvalidOperationException("Stock service is required to complete a BOM production order.");
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            throw new ArgumentException("WarehouseId is required when completing a BOM production order.");

        var components = await ResolveComponentsWithAlternatesAsync(bom, qty, warehouseId.Value, tenantId);
        if (components.Count == 0)
            throw new InvalidOperationException("BOM has no active component lines.");

        var issued = await _stock.IssueStockAsync(tenantId, warehouseId.Value, components, userId.ToString());
        if (!issued)
            throw new InvalidOperationException("Insufficient component stock to complete production.");

        var fgItemId = order.ItemId ?? bom.FinishedGoodItemId;
        var received = await _stock.ReceiveStockAsync(
            tenantId, warehouseId.Value, [(fgItemId, qty)], userId.ToString());
        if (!received)
            throw new InvalidOperationException("Failed to receive finished-good stock.");
    }

    /// <summary>
    /// Prefer primary line; if short and AlternateGroup set, use first alternate with enough stock.
    /// </summary>
    private async Task<List<(Guid ItemId, decimal Quantity)>> ResolveComponentsWithAlternatesAsync(
        BillOfMaterial bom, decimal qty, Guid warehouseId, Guid tenantId)
    {
        var active = bom.Lines.Where(l => l.IsActive).OrderBy(l => l.LineNumber).ToList();
        var result = new List<(Guid, decimal)>();
        var usedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in active)
        {
            if (!string.IsNullOrWhiteSpace(line.AlternateGroup)
                && usedGroups.Contains(line.AlternateGroup))
                continue;

            var need = line.QuantityPer * qty;
            if (line.ScrapPercent > 0)
                need *= 1m + (line.ScrapPercent / 100m);
            need = Math.Round(need, 4);

            var pick = line.ComponentItemId;
            if (_stock != null && !string.IsNullOrWhiteSpace(line.AlternateGroup))
            {
                var avail = await AvailableAsync(tenantId, warehouseId, line.ComponentItemId);
                if (avail < need)
                {
                    var alts = active.Where(a =>
                        string.Equals(a.AlternateGroup, line.AlternateGroup, StringComparison.OrdinalIgnoreCase)
                        && a.Id != line.Id).ToList();
                    foreach (var alt in alts)
                    {
                        var altNeed = Math.Round(alt.QuantityPer * qty * (1m + (alt.ScrapPercent / 100m)), 4);
                        if (await AvailableAsync(tenantId, warehouseId, alt.ComponentItemId) >= altNeed)
                        {
                            pick = alt.ComponentItemId;
                            need = altNeed;
                            break;
                        }
                    }
                }
                usedGroups.Add(line.AlternateGroup);
            }

            result.Add((pick, need));
        }

        return result;
    }

    private async Task<decimal> AvailableAsync(Guid tenantId, Guid warehouseId, Guid itemId)
    {
        var bal = await _db.StockBalances.AsNoTracking()
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.WarehouseId == warehouseId && b.ItemId == itemId);
        return bal?.AvailableQuantity ?? 0m;
    }

    private async Task MarkMaterialLinesConsumedAsync(ProductionOrder order, decimal qty, Guid tenantId, Guid userId)
    {
        var lines = (await _productionOrderLineRepository.GetByProductionOrderAsync(order.Id, tenantId))
            .Where(l => string.Equals(l.LineType, "Material", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var line in lines)
        {
            var scale = order.Quantity > 0 ? qty / order.Quantity : 1m;
            line.ConsumedQuantity = Math.Round(line.Quantity * scale, 4);
            line.UpdatedAt = DateTime.UtcNow;
            line.UpdatedBy = userId.ToString();
            await _productionOrderLineRepository.UpdateAsync(line);
        }
    }

    private async Task LoadChildrenAsync(ProductionOrder order, Guid tenantId)
    {
        order.ProductionOrderLines = (await _productionOrderLineRepository.GetByProductionOrderAsync(order.Id, tenantId)).ToList();
        order.Operations = await _db.ProductionOrderOperations
            .Where(o => o.ProductionOrderId == order.Id && o.TenantId == tenantId && o.IsActive)
            .OrderBy(o => o.StepNumber)
            .ToListAsync();
    }

    private async Task<ProductionOrder> RequireTenantOrderAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id, tenantId);
        if (productionOrder == null)
            throw new KeyNotFoundException($"Production Order with ID '{id}' not found.");
        return productionOrder;
    }

    private static DateTime StartOfWeekUtc(DateTime utc)
    {
        var d = utc.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }

    private static ProductionOrderOperationDto MapOp(ProductionOrderOperation o) => new()
    {
        Id = o.Id,
        ProductionOrderId = o.ProductionOrderId,
        RoutingStepId = o.RoutingStepId,
        StepNumber = o.StepNumber,
        WorkCenterId = o.WorkCenterId,
        OperationName = o.OperationName,
        StandardTimeMinutes = o.StandardTimeMinutes,
        ActualStart = o.ActualStart,
        ActualEnd = o.ActualEnd,
        ActualTimeMinutes = o.ActualTimeMinutes,
        Status = o.Status,
        Notes = o.Notes
    };

    private static ProductionOrderDto MapToDto(ProductionOrder productionOrder)
    {
        return new ProductionOrderDto
        {
            Id = productionOrder.Id,
            OrderNumber = productionOrder.OrderNumber,
            ProductionOrderNumber = productionOrder.ProductionOrderNumber,
            ItemId = productionOrder.ItemId,
            BomId = productionOrder.BomId,
            RoutingId = productionOrder.RoutingId,
            Quantity = productionOrder.Quantity,
            CompletedQuantity = productionOrder.CompletedQuantity,
            WorkCenterId = productionOrder.WorkCenterId,
            SalesOrderId = productionOrder.SalesOrderId,
            SalesOrderLineId = productionOrder.SalesOrderLineId,
            WarehouseId = productionOrder.WarehouseId,
            PlannedStartDate = productionOrder.PlannedStartDate,
            PlannedEndDate = productionOrder.PlannedEndDate,
            ActualStartDate = productionOrder.ActualStartDate,
            ActualEndDate = productionOrder.ActualEndDate,
            Status = productionOrder.Status,
            Priority = productionOrder.Priority,
            Notes = productionOrder.Notes,
            CancellationReason = productionOrder.CancellationReason,
            TenantId = productionOrder.TenantId,
            CreatedBy = productionOrder.CreatedBy,
            CreatedAt = productionOrder.CreatedAt,
            UpdatedBy = productionOrder.UpdatedBy,
            UpdatedAt = productionOrder.UpdatedAt,
            Lines = productionOrder.ProductionOrderLines?
                .Select(l => new ProductionOrderLineDto
                {
                    Id = l.Id,
                    ProductionOrderId = l.ProductionOrderId,
                    ItemId = l.ItemId,
                    Quantity = l.Quantity,
                    ConsumedQuantity = l.ConsumedQuantity,
                    LineType = l.LineType,
                    Notes = l.Notes
                }).ToList() ?? [],
            Operations = productionOrder.Operations?.OrderBy(o => o.StepNumber).Select(MapOp).ToList() ?? []
        };
    }
}
