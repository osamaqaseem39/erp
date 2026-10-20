using ERP.Application.Interfaces.Manufacturing;
using ERP.Domain.CRM;
using ERP.Domain.Manufacturing;
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
            OrderNumber = $"{order.OrderNumber}-L{line.Id:N}"[..24],
            ItemId = line.ItemId,
            BomId = dto.BomId,
            RoutingId = dto.RoutingId,
            WorkCenterId = dto.WorkCenterId,
            Quantity = qty,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            Notes = $"Released from SO {order.OrderNumber}, line {line.ItemName}"
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

        var released = await _db.ProductionOrders.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive && p.Notes != null)
            .Select(p => new { p.Notes, p.Quantity })
            .ToListAsync();

        var result = new List<PendingSalesOrderForProductionDto>();
        foreach (var line in lines)
        {
            var order = openOrders.First(o => o.Id == line.SalesOrderId);
            var tag = $"SO {order.OrderNumber}";
            var releasedQty = released
                .Where(r => r.Notes!.Contains(tag, StringComparison.OrdinalIgnoreCase)
                    && r.Notes.Contains(line.ItemName, StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Quantity);
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
}
