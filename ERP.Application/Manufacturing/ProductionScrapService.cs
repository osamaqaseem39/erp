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

    public ProductionScrapService(ERPDbContext db) => _db = db;

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

        var po = await _db.ProductionOrders.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductionOrderId && p.TenantId == tenantId);
        if (po == null) throw new NotFoundException("Production order not found.");

        var entity = new ProductionScrap
        {
            TenantId = tenantId,
            ProductionOrderId = dto.ProductionOrderId,
            ItemId = dto.ItemId ?? po.ItemId,
            Quantity = dto.Quantity,
            ScrapDate = dto.ScrapDate ?? DateTime.UtcNow,
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
        Reason = e.Reason
    };
}
