using ERP.Application.Interfaces.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Manufacturing;

public interface IManufacturingReleaseService
{
    Task<ProductionOrderDto> ReleaseSalesOrderLineAsync(ReleaseSalesOrderLineDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<PendingSalesOrderForProductionDto>> GetPendingSalesOrdersAsync(Guid tenantId);
}

public class ManufacturingReleaseService : IManufacturingReleaseService
{
    private readonly ERPDbContext _db;
    private readonly IProductionOrderService _productionOrders;

    public ManufacturingReleaseService(ERPDbContext db, IProductionOrderService productionOrders)
    {
        _db = db;
        _productionOrders = productionOrders;
    }

    public async Task<ProductionOrderDto> ReleaseSalesOrderLineAsync(ReleaseSalesOrderLineDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.SalesOrderLineId == Guid.Empty) throw new ValidationException("Sales order line is required.");

        var line = await _db.SalesOrderLines.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == dto.SalesOrderLineId && l.TenantId == tenantId)
            ?? throw new NotFoundException("Sales order line not found.");
        var order = await _db.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == line.SalesOrderId && o.TenantId == tenantId)
            ?? throw new NotFoundException("Sales order not found.");

        var qty = dto.Quantity ?? line.Quantity;
        if (qty <= 0) throw new ValidationException("Quantity must be positive.");

        return await _productionOrders.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = Truncate($"{order.OrderNumber}-L{line.Id:N}", 50),
            ItemId = line.ItemId,
            BomId = dto.BomId,
            RoutingId = dto.RoutingId,
            WorkCenterId = dto.WorkCenterId,
            WarehouseId = dto.WarehouseId,
            SalesOrderId = order.Id,
            SalesOrderLineId = line.Id,
            Quantity = qty,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            Status = "Released",
            Notes = $"Released from SO {order.OrderNumber}"
        }, tenantId, userId);
    }

    public async Task<IReadOnlyList<PendingSalesOrderForProductionDto>> GetPendingSalesOrdersAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PendingSalesOrderForProductionDto>();

        var openOrders = await _db.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.IsActive
                && (o.Status == "Confirmed" || o.Status == "Open" || o.Status == "Approved"))
            .ToListAsync();
        if (openOrders.Count == 0) return Array.Empty<PendingSalesOrderForProductionDto>();

        var orderIds = openOrders.Select(o => o.Id).ToList();
        var lines = await _db.SalesOrderLines.AsNoTracking()
            .Where(l => l.TenantId == tenantId && orderIds.Contains(l.SalesOrderId))
            .ToListAsync();

        var lineIds = lines.Select(l => l.Id).ToList();
        var releasedByLine = await _db.ProductionOrders.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive
                && p.SalesOrderLineId != null
                && lineIds.Contains(p.SalesOrderLineId.Value)
                && p.Status != "Cancelled")
            .GroupBy(p => p.SalesOrderLineId!.Value)
            .Select(g => new { LineId = g.Key, Qty = g.Sum(p => p.Quantity) })
            .ToListAsync();
        var releasedMap = releasedByLine.ToDictionary(x => x.LineId, x => x.Qty);

        var result = new List<PendingSalesOrderForProductionDto>();
        foreach (var line in lines)
        {
            var order = openOrders.First(o => o.Id == line.SalesOrderId);
            releasedMap.TryGetValue(line.Id, out var releasedQty);
            var pending = Math.Max(0, line.Quantity - releasedQty);
            if (pending <= 0) continue;
            result.Add(new PendingSalesOrderForProductionDto
            {
                SalesOrderId = order.Id,
                OrderNumber = order.OrderNumber,
                SalesOrderLineId = line.Id,
                ItemId = line.ItemId,
                ItemName = line.ItemName,
                Quantity = line.Quantity,
                ReleasedQuantity = releasedQty,
                PendingQuantity = pending
            });
        }

        return result.OrderBy(r => r.OrderNumber).ToList();
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
