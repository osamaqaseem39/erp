using ERP.Application.Interfaces.Manufacturing;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Manufacturing;

public class ProductionRunService : IProductionRunService
{
    private readonly IProductionRunRepository _runs;
    private readonly IProductionOrderRepository _orders;

    public ProductionRunService(IProductionRunRepository runs, IProductionOrderRepository orders)
    {
        _runs = runs;
        _orders = orders;
    }

    public async Task<ProductionRunDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _runs.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ProductionRunDto>> GetByOrderAsync(Guid productionOrderId, Guid tenantId)
    {
        var order = await _orders.GetByIdAsync(productionOrderId, tenantId);
        if (order == null) return [];
        return (await _runs.GetByProductionOrderAsync(productionOrderId, tenantId)).Select(Map);
    }

    public async Task<ProductionRunDto> CreateAsync(CreateProductionRunDto dto, Guid tenantId, Guid userId)
    {
        var order = await _orders.GetByIdAsync(dto.ProductionOrderId, tenantId)
            ?? throw new KeyNotFoundException("Production order not found.");
        if (string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot start a run on a closed production order.");

        if (string.Equals(order.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            order.Status = "In Progress";
            order.ActualStartDate ??= DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId.ToString();
            await _orders.UpdateAsync(order);
        }

        var entity = new ProductionRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductionOrderId = dto.ProductionOrderId,
            RunNumber = string.IsNullOrWhiteSpace(dto.RunNumber)
                ? $"RUN-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.RunNumber.Trim(),
            StartTime = dto.StartTime ?? DateTime.UtcNow,
            QuantityProduced = dto.QuantityProduced ?? 0,
            QuantityRejected = dto.QuantityRejected ?? 0,
            Status = "InProgress",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        await _runs.CreateAsync(entity);
        return Map(entity);
    }

    public async Task<ProductionRunDto> CompleteAsync(
        Guid id, decimal quantityProduced, decimal quantityRejected, Guid tenantId, Guid userId)
    {
        var e = await _runs.GetByIdAsync(id, tenantId)
            ?? throw new KeyNotFoundException("Production run not found.");
        if (!string.Equals(e.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only in-progress runs can be completed.");

        e.QuantityProduced = quantityProduced;
        e.QuantityRejected = quantityRejected;
        e.EndTime = DateTime.UtcNow;
        e.Status = "Completed";
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId.ToString();
        await _runs.UpdateAsync(e);

        var order = await _orders.GetByIdAsync(e.ProductionOrderId, tenantId);
        if (order != null)
        {
            var runs = (await _runs.GetByProductionOrderAsync(order.Id, tenantId)).ToList();
            var produced = runs.Where(r => r.Status == "Completed").Sum(r => r.QuantityProduced);
            if (produced >= order.Quantity && order.Quantity > 0)
            {
                order.Status = "Completed";
                order.ActualEndDate = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = userId.ToString();
                await _orders.UpdateAsync(order);
            }
        }

        return Map(e);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var e = await _runs.GetByIdAsync(id, tenantId);
        if (e == null) return false;
        if (string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase)) return false;
        e.Status = "Cancelled";
        e.EndTime = DateTime.UtcNow;
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId.ToString();
        return await _runs.UpdateAsync(e);
    }

    private static ProductionRunDto Map(ProductionRun e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProductionOrderId = e.ProductionOrderId,
        RunNumber = e.RunNumber,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        QuantityProduced = e.QuantityProduced,
        QuantityRejected = e.QuantityRejected,
        Status = e.Status,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
