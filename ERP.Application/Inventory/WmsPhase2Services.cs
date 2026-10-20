using ERP.Application.Common;
using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Inventory;

public class ReplenishmentTaskService : IReplenishmentTaskService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = ["InProgress", "Completed", "Cancelled"],
            ["InProgress"] = ["Completed", "Cancelled"],
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IReplenishmentTaskRepository _repo;
    private readonly IWarehouseLocationRepository _locations;
    private readonly ERP.Application.Interfaces.IStockService _stock;

    public ReplenishmentTaskService(
        IReplenishmentTaskRepository repo,
        IWarehouseLocationRepository locations,
        ERP.Application.Interfaces.IStockService stock)
    {
        _repo = repo;
        _locations = locations;
        _stock = stock;
    }

    public async Task<ReplenishmentTaskDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ReplenishmentTaskDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreateReplenishmentTaskDto dto, Guid tenantId, Guid userId)
    {
        if (dto.WarehouseId == Guid.Empty || dto.Lines.Count == 0) return Guid.Empty;
        var bulk = await PreferLocationAsync(dto.WarehouseId, tenantId, "Bulk");
        var pick = await PreferLocationAsync(dto.WarehouseId, tenantId, "Pick");
        var id = Guid.NewGuid();
        var entity = new ReplenishmentTask
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            TaskNumber = string.IsNullOrWhiteSpace(dto.TaskNumber)
                ? $"RPL-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.TaskNumber.Trim(),
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Where(l => l.Quantity > 0).Select(l => new ReplenishmentTaskLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReplenishmentTaskId = id,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                FromLocationId = l.FromLocationId ?? bulk,
                ToLocationId = l.ToLocationId ?? pick,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        if (entity.Lines.Count == 0) return Guid.Empty;
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "InProgress", Transitions)) return false;
        entity.Status = "InProgress";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Completed", Transitions)) return false;
        if (entity.Lines.Count == 0) return false;
        if (entity.Lines.Any(l =>
                !l.FromLocationId.HasValue || l.FromLocationId == Guid.Empty
                || !l.ToLocationId.HasValue || l.ToLocationId == Guid.Empty
                || l.FromLocationId == l.ToLocationId))
            return false;

        var issue = entity.Lines.Select(l => (l.ItemId, l.Quantity, l.FromLocationId)).ToList();
        if (!await _stock.IssueStockAsync(tenantId, entity.WarehouseId, issue, userId.ToString()))
            return false;
        var receive = entity.Lines.Select(l => (l.ItemId, l.Quantity, l.ToLocationId)).ToList();
        if (!await _stock.ReceiveStockAsync(tenantId, entity.WarehouseId, receive, userId.ToString()))
            return false;

        entity.Status = "Completed";
        entity.CompletedAt = DateTime.UtcNow;
        entity.CompletedBy = userId.ToString();
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions)) return false;
        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private async Task<Guid?> PreferLocationAsync(Guid warehouseId, Guid tenantId, string type)
    {
        var locs = (await _locations.GetByWarehouseAsync(warehouseId, tenantId)).ToList();
        return locs.FirstOrDefault(l => string.Equals(l.LocationType, type, StringComparison.OrdinalIgnoreCase))?.Id
               ?? locs.FirstOrDefault()?.Id;
    }

    private static ReplenishmentTaskDto Map(ReplenishmentTask e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        TaskNumber = e.TaskNumber,
        WarehouseId = e.WarehouseId,
        Status = e.Status,
        Notes = e.Notes,
        CompletedAt = e.CompletedAt,
        CompletedBy = e.CompletedBy,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new ReplenishmentTaskLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            ReplenishmentTaskId = l.ReplenishmentTaskId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            FromLocationId = l.FromLocationId,
            ToLocationId = l.ToLocationId,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}

public class PackOrderService : IPackOrderService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = ["Packed", "Cancelled"],
            ["Packed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IPackOrderRepository _repo;
    private readonly IPickListRepository _picks;
    private readonly IShipmentService _shipments;

    public PackOrderService(IPackOrderRepository repo, IPickListRepository picks, IShipmentService shipments)
    {
        _repo = repo;
        _picks = picks;
        _shipments = shipments;
    }

    public async Task<PackOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<PackOrderDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreatePackOrderDto dto, Guid tenantId, Guid userId)
    {
        if (dto.WarehouseId == Guid.Empty || dto.Lines.Count == 0) return Guid.Empty;
        var id = Guid.NewGuid();
        var entity = new PackOrder
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            PickListId = dto.PickListId,
            DispatchAdviceId = dto.DispatchAdviceId,
            PackNumber = string.IsNullOrWhiteSpace(dto.PackNumber)
                ? $"PACK-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.PackNumber.Trim(),
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Where(l => l.Quantity > 0).Select(l => new PackOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PackOrderId = id,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                PackageCode = l.PackageCode,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        if (entity.Lines.Count == 0) return Guid.Empty;
        return await _repo.CreateAsync(entity);
    }

    public async Task<Guid> CreateFromPickAsync(Guid pickListId, Guid tenantId, Guid userId)
    {
        var pick = await _picks.GetByIdAsync(pickListId, tenantId);
        if (pick == null || !string.Equals(pick.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;
        Guid? dispatchId = string.Equals(pick.ReferenceDocType, "DispatchAdvice", StringComparison.OrdinalIgnoreCase)
            ? pick.ReferenceDocId
            : null;
        return await CreateAsync(new CreatePackOrderDto
        {
            WarehouseId = pick.WarehouseId,
            PickListId = pick.Id,
            DispatchAdviceId = dispatchId,
            Notes = $"From pick {pick.PickNumber}",
            Lines = pick.Lines.Select(l => new CreatePackOrderLineDto
            {
                ItemId = l.ItemId,
                Quantity = l.QuantityPicked > 0 ? l.QuantityPicked : l.QuantityRequested
            }).ToList()
        }, tenantId, userId);
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Packed", Transitions)) return false;
        entity.Status = "Packed";
        entity.PackedAt = DateTime.UtcNow;
        entity.PackedBy = userId.ToString();
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        if (!await _repo.UpdateAsync(entity)) return false;

        await _shipments.CreateFromPackAsync(entity.Id, tenantId, userId);
        return true;
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions)) return false;
        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    public async Task<bool> HasPackedForDispatchAsync(Guid dispatchId, Guid tenantId)
        => await _repo.GetPackedByDispatchAsync(dispatchId, tenantId) != null;

    private static PackOrderDto Map(PackOrder e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        PackNumber = e.PackNumber,
        WarehouseId = e.WarehouseId,
        PickListId = e.PickListId,
        DispatchAdviceId = e.DispatchAdviceId,
        Status = e.Status,
        Notes = e.Notes,
        PackedAt = e.PackedAt,
        PackedBy = e.PackedBy,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new PackOrderLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            PackOrderId = l.PackOrderId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            PackageCode = l.PackageCode,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}

public class ShipmentService : IShipmentService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = ["Shipped", "Cancelled"],
            ["Shipped"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IShipmentRepository _repo;
    private readonly IPackOrderRepository _packs;

    public ShipmentService(IShipmentRepository repo, IPackOrderRepository packs)
    {
        _repo = repo;
        _packs = packs;
    }

    public async Task<ShipmentDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ShipmentDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreateShipmentDto dto, Guid tenantId, Guid userId)
    {
        if (dto.WarehouseId == Guid.Empty || dto.Lines.Count == 0) return Guid.Empty;
        var id = Guid.NewGuid();
        var entity = new Shipment
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            PackOrderId = dto.PackOrderId,
            DispatchAdviceId = dto.DispatchAdviceId,
            ShipmentNumber = string.IsNullOrWhiteSpace(dto.ShipmentNumber)
                ? $"SHP-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.ShipmentNumber.Trim(),
            Status = "Draft",
            Carrier = dto.Carrier,
            TrackingNumber = dto.TrackingNumber,
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Where(l => l.Quantity > 0).Select(l => new ShipmentLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ShipmentId = id,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        if (entity.Lines.Count == 0) return Guid.Empty;
        return await _repo.CreateAsync(entity);
    }

    public async Task<Guid> CreateFromPackAsync(Guid packOrderId, Guid tenantId, Guid userId)
    {
        var pack = await _packs.GetByIdAsync(packOrderId, tenantId);
        if (pack == null || !string.Equals(pack.Status, "Packed", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;
        return await CreateAsync(new CreateShipmentDto
        {
            WarehouseId = pack.WarehouseId,
            PackOrderId = pack.Id,
            DispatchAdviceId = pack.DispatchAdviceId,
            Notes = $"From pack {pack.PackNumber}",
            Lines = pack.Lines.Select(l => new CreateShipmentLineDto
            {
                ItemId = l.ItemId,
                Quantity = l.Quantity
            }).ToList()
        }, tenantId, userId);
    }

    public async Task<bool> ShipAsync(Guid id, ShipShipmentDto? dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Shipped", Transitions)) return false;
        if (dto != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Carrier)) entity.Carrier = dto.Carrier.Trim();
            if (!string.IsNullOrWhiteSpace(dto.TrackingNumber)) entity.TrackingNumber = dto.TrackingNumber.Trim();
        }
        entity.Status = "Shipped";
        entity.ShippedAt = DateTime.UtcNow;
        entity.ShippedBy = userId.ToString();
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions)) return false;
        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    public async Task<bool> HasShippedForDispatchAsync(Guid dispatchId, Guid tenantId)
        => await _repo.GetShippedByDispatchAsync(dispatchId, tenantId) != null;

    private static ShipmentDto Map(Shipment e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ShipmentNumber = e.ShipmentNumber,
        WarehouseId = e.WarehouseId,
        PackOrderId = e.PackOrderId,
        DispatchAdviceId = e.DispatchAdviceId,
        Status = e.Status,
        Carrier = e.Carrier,
        TrackingNumber = e.TrackingNumber,
        Notes = e.Notes,
        ShippedAt = e.ShippedAt,
        ShippedBy = e.ShippedBy,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new ShipmentLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            ShipmentId = l.ShipmentId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}
