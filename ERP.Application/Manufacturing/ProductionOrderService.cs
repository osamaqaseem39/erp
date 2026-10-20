using ERP.Application.Interfaces;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Manufacturing;

public class ProductionOrderService : IProductionOrderService
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionOrderLineRepository _productionOrderLineRepository;
    private readonly IProductionRunRepository _productionRunRepository;
    private readonly IWorkCenterRepository _workCenterRepository;
    private readonly IBillOfMaterialRepository _boms;
    private readonly IStockService? _stock;

    public ProductionOrderService(
        IProductionOrderRepository productionOrderRepository,
        IProductionOrderLineRepository productionOrderLineRepository,
        IProductionRunRepository productionRunRepository,
        IWorkCenterRepository workCenterRepository,
        IBillOfMaterialRepository boms,
        IStockService? stock = null)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionOrderLineRepository = productionOrderLineRepository;
        _productionRunRepository = productionRunRepository;
        _workCenterRepository = workCenterRepository;
        _boms = boms;
        _stock = stock;
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id, tenantId);
        return productionOrder != null ? MapToDto(productionOrder) : null;
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
        if (dto.BomId.HasValue && dto.BomId != Guid.Empty)
        {
            var bom = await _boms.GetByIdAsync(dto.BomId.Value, tenantId)
                ?? throw new ArgumentException("BOM not found.");
            if (!string.Equals(bom.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("BOM must be Active.");
            dto.ItemId ??= bom.FinishedGoodItemId;
        }

        var productionOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = dto.OrderNumber ?? Guid.NewGuid().ToString("N")[..8],
            ProductionOrderNumber = dto.ProductionOrderNumber ?? string.Empty,
            ItemId = dto.ItemId,
            BomId = dto.BomId == Guid.Empty ? null : dto.BomId,
            RoutingId = dto.RoutingId == Guid.Empty ? null : dto.RoutingId,
            Quantity = dto.Quantity ?? 0m,
            WorkCenterId = dto.WorkCenterId,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            Status = "Draft",
            Priority = dto.Priority ?? "Medium",
            Notes = dto.Notes,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _productionOrderRepository.CreateAsync(productionOrder);
        var created = await _productionOrderRepository.GetByIdAsync(createdId, tenantId);
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
        productionOrder.PlannedStartDate = dto.PlannedStartDate ?? productionOrder.PlannedStartDate;
        productionOrder.PlannedEndDate = dto.PlannedEndDate ?? productionOrder.PlannedEndDate;
        productionOrder.Priority = dto.Priority ?? productionOrder.Priority;
        productionOrder.Notes = dto.Notes ?? productionOrder.Notes;
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

        if (productionOrder.Status != "Draft")
            throw new InvalidOperationException("Only draft production orders can be started.");

        productionOrder.Status = "In Progress";
        productionOrder.ActualStartDate = DateTime.UtcNow;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);

        var run = new ProductionRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductionOrderId = productionOrder.Id,
            RunNumber = $"RUN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            StartTime = DateTime.UtcNow,
            Status = "InProgress",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _productionRunRepository.CreateAsync(run);

        return MapToDto(productionOrder);
    }

    public async Task<ProductionOrderDto> CompleteAsync(Guid id, CompleteProductionOrderDto dto, Guid tenantId, Guid userId)
    {
        var productionOrder = await RequireTenantOrderAsync(id, tenantId);

        if (productionOrder.Status != "In Progress")
            throw new InvalidOperationException("Only in-progress production orders can be completed.");

        var qty = dto.ActualQuantity > 0 ? dto.ActualQuantity : productionOrder.Quantity;
        if (qty <= 0)
            throw new ArgumentException("Completion quantity must be greater than zero.");

        await ApplyBomStockAsync(productionOrder, qty, dto.WarehouseId, tenantId, userId);

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

    private async Task ApplyBomStockAsync(
        ProductionOrder order, decimal qty, Guid? warehouseId, Guid tenantId, Guid userId)
    {
        BillOfMaterial? bom = null;
        if (order.BomId.HasValue && order.BomId != Guid.Empty)
            bom = await _boms.GetByIdAsync(order.BomId.Value, tenantId);
        else if (order.ItemId.HasValue)
            bom = await _boms.GetActiveByFinishedGoodAsync(order.ItemId.Value, tenantId);

        if (bom == null) return; // no BOM → status-only complete (legacy)

        if (_stock == null)
            throw new InvalidOperationException("Stock service is required to complete a BOM production order.");
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            throw new ArgumentException("WarehouseId is required when completing a BOM production order.");

        var components = bom.Lines.Where(l => l.IsActive).Select(l =>
        {
            var need = l.QuantityPer * qty;
            if (l.ScrapPercent > 0)
                need *= 1m + (l.ScrapPercent / 100m);
            return (l.ComponentItemId, Math.Round(need, 4));
        }).ToList();

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

    private async Task<ProductionOrder> RequireTenantOrderAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id, tenantId);
        if (productionOrder == null)
            throw new KeyNotFoundException($"Production Order with ID '{id}' not found.");
        return productionOrder;
    }

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
            UpdatedAt = productionOrder.UpdatedAt
        };
    }
}
