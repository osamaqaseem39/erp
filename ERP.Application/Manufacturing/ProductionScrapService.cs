using ERP.Application.Interfaces;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Manufacturing;

public interface IProductionScrapService
{
    Task<IReadOnlyList<ProductionScrapDto>> ListAsync(Guid tenantId, Guid? productionOrderId = null);
    Task<ProductionScrapDto> CreateAsync(CreateProductionScrapDto dto, Guid tenantId, Guid userId);
}

public class ProductionScrapService : IProductionScrapService
{
    private readonly ERPDbContext _db;
    private readonly IStockService? _stock;

    public ProductionScrapService(ERPDbContext db, IStockService? stock = null)
    {
        _db = db;
        _stock = stock;
    }

    public async Task<IReadOnlyList<ProductionScrapDto>> ListAsync(Guid tenantId, Guid? productionOrderId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionScrapDto>();
        var q = _db.ProductionScraps.AsNoTracking().Where(s => s.TenantId == tenantId && s.IsActive);
        if (productionOrderId.HasValue && productionOrderId != Guid.Empty)
            q = q.Where(s => s.ProductionOrderId == productionOrderId);
        return (await q.OrderByDescending(s => s.ScrapDate).ToListAsync()).Select(Map).ToList();
    }

    public async Task<ProductionScrapDto> CreateAsync(CreateProductionScrapDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.ProductionOrderId == Guid.Empty) throw new ValidationException("Production order is required.");
        if (dto.Quantity <= 0) throw new ValidationException("Quantity must be positive.");
        if (!dto.ScrapReasonId.HasValue && string.IsNullOrWhiteSpace(dto.Reason))
            throw new ValidationException("ScrapReasonId or Reason is required.");

        var po = await _db.ProductionOrders
            .FirstOrDefaultAsync(p => p.Id == dto.ProductionOrderId && p.TenantId == tenantId)
            ?? throw new NotFoundException("Production order not found.");

        if (dto.ScrapReasonId.HasValue)
        {
            var reasonOk = await _db.ScrapReasons.AsNoTracking()
                .AnyAsync(r => r.Id == dto.ScrapReasonId && r.TenantId == tenantId && r.IsActive);
            if (!reasonOk) throw new ValidationException("Scrap reason not found.");
        }

        var warehouseId = dto.WarehouseId ?? po.WarehouseId;
        var itemId = dto.ItemId ?? po.ItemId;
        if (!itemId.HasValue || itemId == Guid.Empty)
            throw new ValidationException("ItemId is required for scrap.");
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            throw new ValidationException("WarehouseId is required to scrap stock.");
        if (_stock == null)
            throw new InvalidOperationException("Stock service is required to record scrap.");

        var issued = await _stock.IssueStockAsync(
            tenantId, warehouseId.Value, [(itemId.Value, dto.Quantity)], userId.ToString());
        if (!issued)
            throw new InvalidOperationException("Insufficient stock to record scrap.");

        var entity = new ProductionScrap
        {
            TenantId = tenantId,
            ProductionOrderId = dto.ProductionOrderId,
            ItemId = itemId,
            Quantity = dto.Quantity,
            ScrapDate = dto.ScrapDate ?? DateTime.UtcNow,
            ScrapReasonId = dto.ScrapReasonId,
            WarehouseId = warehouseId,
            Reason = dto.Reason,
            CreatedBy = userId.ToString()
        };
        _db.ProductionScraps.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static ProductionScrapDto Map(ProductionScrap e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProductionOrderId = e.ProductionOrderId,
        ItemId = e.ItemId,
        Quantity = e.Quantity,
        ScrapDate = e.ScrapDate,
        ScrapReasonId = e.ScrapReasonId,
        WarehouseId = e.WarehouseId,
        Reason = e.Reason
    };
}
