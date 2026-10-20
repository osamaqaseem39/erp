using ERP.Application.Common;
using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Inventory;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;
    public BrandService(IBrandRepository repo) => _repo = repo;

    public async Task<BrandDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapBrand(e);
    }

    public async Task<IEnumerable<BrandDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
        => (await _repo.GetAllAsync(tenantId, isActive)).Select(MapBrand);

    public async Task<BrandDto> CreateAsync(CreateBrandDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        var entity = new Brand
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        return MapBrand(await _repo.CreateAsync(entity));
    }

    public async Task<BrandDto?> UpdateAsync(UpdateBrandDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return null;
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return MapBrand(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static BrandDto MapBrand(Brand e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy
    };
}

public class WarehouseLocationService : IWarehouseLocationService
{
    private readonly IWarehouseLocationRepository _repo;
    public WarehouseLocationService(IWarehouseLocationRepository repo) => _repo = repo;

    public async Task<WarehouseLocationDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapLoc(e);
    }

    public async Task<IEnumerable<WarehouseLocationDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
        => (await _repo.GetByWarehouseAsync(warehouseId, tenantId)).Select(MapLoc);

    public async Task<WarehouseLocationDto> CreateAsync(CreateWarehouseLocationDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        var entity = new WarehouseLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            LocationType = NormalizeLocationType(dto.LocationType),
            Zone = string.IsNullOrWhiteSpace(dto.Zone) ? null : dto.Zone.Trim(),
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        return MapLoc(await _repo.CreateAsync(entity));
    }

    public async Task<WarehouseLocationDto?> UpdateAsync(UpdateWarehouseLocationDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return null;
        entity.WarehouseId = dto.WarehouseId;
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.LocationType = NormalizeLocationType(dto.LocationType);
        entity.Zone = string.IsNullOrWhiteSpace(dto.Zone) ? null : dto.Zone.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return MapLoc(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static string NormalizeLocationType(string? type)
    {
        var t = string.IsNullOrWhiteSpace(type) ? "Pick" : type.Trim();
        return t switch
        {
            "Bulk" or "Staging" or "QC" or "Pick" => t,
            _ => "Pick"
        };
    }

    private static WarehouseLocationDto MapLoc(WarehouseLocation e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        WarehouseId = e.WarehouseId,
        Code = e.Code,
        Name = e.Name,
        LocationType = e.LocationType,
        Zone = e.Zone,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy
    };
}

public class StockReservationService : IStockReservationService
{
    private readonly IStockReservationRepository _repo;
    private readonly IStockService _stock;

    public StockReservationService(IStockReservationRepository repo, IStockService stock)
    {
        _repo = repo;
        _stock = stock;
    }

    public async Task<StockReservationDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapRes(e);
    }

    public async Task<IEnumerable<StockReservationDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(MapRes);

    public async Task<IEnumerable<StockReservationDto>> GetBySalesOrderAsync(Guid salesOrderId, Guid tenantId)
        => (await _repo.GetBySalesOrderAsync(salesOrderId, tenantId)).Select(MapRes);

    public async Task<Guid> CreateAsync(CreateStockReservationDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.Lines.Count == 0) return Guid.Empty;

        var reserved = new List<(Guid WarehouseId, Guid ItemId, decimal Quantity)>();
        foreach (var line in dto.Lines)
        {
            if (line.WarehouseId == Guid.Empty || line.ItemId == Guid.Empty || line.Quantity <= 0)
            {
                await UndoReservesAsync(tenantId, reserved, userId);
                return Guid.Empty;
            }

            var ok = await _stock.ReserveAsync(
                tenantId, line.WarehouseId, line.ItemId, line.Quantity, userId.ToString());
            if (!ok)
            {
                await UndoReservesAsync(tenantId, reserved, userId);
                return Guid.Empty;
            }
            reserved.Add((line.WarehouseId, line.ItemId, line.Quantity));
        }

        var id = Guid.NewGuid();
        var entity = new StockReservation
        {
            Id = id,
            TenantId = tenantId,
            SalesOrderId = dto.SalesOrderId,
            ReservationNumber = string.IsNullOrWhiteSpace(dto.ReservationNumber)
                ? $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.ReservationNumber.Trim(),
            Status = "Active",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new StockReservationLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StockReservationId = id,
                SalesOrderId = l.SalesOrderId ?? dto.SalesOrderId,
                ItemId = l.ItemId,
                WarehouseId = l.WarehouseId,
                Quantity = l.Quantity,
                Status = string.IsNullOrWhiteSpace(l.Status) ? "Active" : l.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> ReleaseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (IsTerminal(entity.Status)) return true;

        foreach (var line in entity.Lines.Where(IsActiveLine))
        {
            await _stock.UnreserveAsync(
                tenantId, line.WarehouseId, line.ItemId, line.Quantity, userId.ToString());
            line.Status = "Released";
        }

        entity.Status = "Released";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (IsTerminal(entity.Status)) return true;

        foreach (var line in entity.Lines.Where(IsActiveLine))
        {
            await _stock.UnreserveAsync(
                tenantId, line.WarehouseId, line.ItemId, line.Quantity, userId.ToString());
            line.Status = "Cancelled";
        }

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    private async Task UndoReservesAsync(
        Guid tenantId,
        List<(Guid WarehouseId, Guid ItemId, decimal Quantity)> reserved,
        Guid userId)
    {
        foreach (var (warehouseId, itemId, quantity) in reserved)
            await _stock.UnreserveAsync(tenantId, warehouseId, itemId, quantity, userId.ToString());
    }

    private static bool IsActiveLine(StockReservationLine line) =>
        string.Equals(line.Status, "Active", StringComparison.OrdinalIgnoreCase);

    private static bool IsTerminal(string status) =>
        string.Equals(status, "Released", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static StockReservationDto MapRes(StockReservation e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ReservationNumber = e.ReservationNumber,
        SalesOrderId = e.SalesOrderId,
        Status = e.Status,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new StockReservationLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            StockReservationId = l.StockReservationId,
            SalesOrderId = l.SalesOrderId,
            ItemId = l.ItemId,
            WarehouseId = l.WarehouseId,
            Quantity = l.Quantity,
            Status = l.Status,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}

public class ItemKitService : IItemKitService
{
    private readonly IItemKitRepository _repo;
    public ItemKitService(IItemKitRepository repo) => _repo = repo;

    public async Task<ItemKitDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapKit(e);
    }

    public async Task<ItemKitDto?> GetByKitItemIdAsync(Guid kitItemId, Guid tenantId)
    {
        var e = await _repo.GetByKitItemIdAsync(kitItemId, tenantId);
        return e == null ? null : MapKit(e);
    }

    public async Task<IEnumerable<ItemKitDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(MapKit);

    public async Task<Guid> CreateAsync(CreateItemKitDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.Components.Count == 0) return Guid.Empty;
        var id = Guid.NewGuid();
        var entity = new ItemKit
        {
            Id = id,
            TenantId = tenantId,
            KitItemId = dto.KitItemId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Components = dto.Components.Select(c => new ItemKitComponent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ItemKitId = id,
                ComponentItemId = c.ComponentItemId,
                Quantity = c.Quantity,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateItemKitDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null || dto.Components.Count == 0) return false;
        entity.KitItemId = dto.KitItemId;
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Components.Clear();
        foreach (var c in dto.Components)
        {
            entity.Components.Add(new ItemKitComponent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ItemKitId = entity.Id,
                ComponentItemId = c.ComponentItemId,
                Quantity = c.Quantity,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            });
        }
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static ItemKitDto MapKit(ItemKit e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        KitItemId = e.KitItemId,
        Code = e.Code,
        Name = e.Name,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Components = e.Components.Select(c => new ItemKitComponentDto
        {
            Id = c.Id,
            TenantId = c.TenantId,
            ItemKitId = c.ItemKitId,
            ComponentItemId = c.ComponentItemId,
            Quantity = c.Quantity,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            CreatedBy = c.CreatedBy
        }).ToList()
    };
}

public class CycleCountService : ICycleCountService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = ["Counted", "Cancelled"],
            ["Counted"] = ["Posted", "Draft"],
            ["Posted"] = [],
            ["Cancelled"] = []
        };

    private readonly ICycleCountRepository _repo;
    private readonly IStockAdjustmentService _adjustments;

    public CycleCountService(ICycleCountRepository repo, IStockAdjustmentService adjustments)
    {
        _repo = repo;
        _adjustments = adjustments;
    }

    public async Task<CycleCountDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapCount(e);
    }

    public async Task<IEnumerable<CycleCountDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(MapCount);

    public async Task<Guid> CreateAsync(CreateCycleCountDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.Lines.Count == 0) return Guid.Empty;
        var id = Guid.NewGuid();
        var entity = new CycleCount
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            CountDate = dto.CountDate == default ? DateTime.UtcNow.Date : dto.CountDate,
            CountNumber = string.IsNullOrWhiteSpace(dto.CountNumber)
                ? $"CC-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.CountNumber.Trim(),
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new CycleCountLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CycleCountId = id,
                ItemId = l.ItemId,
                WarehouseLocationId = l.WarehouseLocationId,
                QuantityOnHand = l.QuantityOnHand,
                QuantityCounted = l.QuantityCounted,
                QuantityVariance = l.QuantityCounted - l.QuantityOnHand,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateCycleCountDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null || !string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        if (dto.Lines.Count == 0) return false;
        entity.WarehouseId = dto.WarehouseId;
        entity.CountDate = dto.CountDate == default ? entity.CountDate : dto.CountDate;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            entity.Lines.Add(new CycleCountLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CycleCountId = entity.Id,
                ItemId = l.ItemId,
                WarehouseLocationId = l.WarehouseLocationId,
                QuantityOnHand = l.QuantityOnHand,
                QuantityCounted = l.QuantityCounted,
                QuantityVariance = l.QuantityCounted - l.QuantityOnHand,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            });
        }
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> MarkCountedAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Counted", Transitions)) return false;
        entity.Status = "Counted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> PostAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Posted", Transitions)) return false;
        var varianceLines = entity.Lines.Where(l => l.QuantityVariance != 0).ToList();
        if (varianceLines.Count > 0)
        {
            var adjId = await _adjustments.CreateAsync(new CreateStockAdjustmentDto
            {
                WarehouseId = entity.WarehouseId,
                AdjustmentType = "CycleCount",
                Reference = entity.CountNumber,
                Notes = $"From cycle count {entity.CountNumber}",
                Details = varianceLines.Select(l => new CreateStockAdjustmentDetailDto
                {
                    ItemId = l.ItemId,
                    QuantityOnHand = l.QuantityOnHand,
                    QuantityCounted = l.QuantityCounted,
                    Notes = l.Notes
                }).ToList()
            }, tenantId, userId);
            if (adjId == Guid.Empty) return false;
            if (!await _adjustments.PostAsync(adjId, tenantId, userId))
            {
                await _adjustments.DeleteAsync(adjId, tenantId);
                return false;
            }
            entity.PostedStockAdjustmentId = adjId;
        }

        entity.Status = "Posted";
        entity.PostedBy = userId.ToString();
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static CycleCountDto MapCount(CycleCount e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        CountNumber = e.CountNumber,
        WarehouseId = e.WarehouseId,
        CountDate = e.CountDate,
        Status = e.Status,
        Notes = e.Notes,
        PostedStockAdjustmentId = e.PostedStockAdjustmentId,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new CycleCountLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            CycleCountId = l.CycleCountId,
            ItemId = l.ItemId,
            WarehouseLocationId = l.WarehouseLocationId,
            QuantityOnHand = l.QuantityOnHand,
            QuantityCounted = l.QuantityCounted,
            QuantityVariance = l.QuantityVariance,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}
