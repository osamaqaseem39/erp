using ERP.Application.Common;
using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Inventory;

public class PutawayTaskService : IPutawayTaskService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = new[] { "InProgress", "Completed", "Cancelled" },
            ["InProgress"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IPutawayTaskRepository _repo;
    private readonly IWarehouseLocationRepository _locations;
    private readonly IStockService _stock;

    public PutawayTaskService(
        IPutawayTaskRepository repo,
        IWarehouseLocationRepository locations,
        IStockService stock)
    {
        _repo = repo;
        _locations = locations;
        _stock = stock;
    }

    public async Task<PutawayTaskDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<PutawayTaskDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreatePutawayTaskDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.Lines.Count == 0 || dto.WarehouseId == Guid.Empty) return Guid.Empty;
        var id = Guid.NewGuid();
        var suggested = await SuggestBulkOrPickAsync(dto.WarehouseId, tenantId);
        var entity = new PutawayTask
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            TaskNumber = string.IsNullOrWhiteSpace(dto.TaskNumber)
                ? $"PUT-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.TaskNumber.Trim(),
            ReferenceDocType = string.IsNullOrWhiteSpace(dto.ReferenceDocType) ? "GRN" : dto.ReferenceDocType.Trim(),
            ReferenceDocId = dto.ReferenceDocId,
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Where(l => l.Quantity > 0).Select(l => new PutawayTaskLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PutawayTaskId = id,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                SuggestedLocationId = l.SuggestedLocationId ?? suggested,
                ActualLocationId = l.ActualLocationId,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        if (entity.Lines.Count == 0) return Guid.Empty;
        return await _repo.CreateAsync(entity);
    }

    public async Task<Guid> CreateFromGrnAsync(
        Guid grnId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, Guid tenantId, Guid userId)
    {
        var existing = await _repo.GetByReferenceAsync("GRN", grnId, tenantId);
        if (existing != null) return existing.Id;

        return await CreateAsync(new CreatePutawayTaskDto
        {
            WarehouseId = warehouseId,
            ReferenceDocType = "GRN",
            ReferenceDocId = grnId,
            Notes = $"Auto-created from GRN {grnId:N}",
            Lines = lines.Select(l => new CreatePutawayTaskLineDto
            {
                ItemId = l.ItemId,
                Quantity = l.Quantity
            }).ToList()
        }, tenantId, userId);
    }

    public async Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "InProgress", Transitions)) return false;
        entity.Status = "InProgress";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> CompleteAsync(Guid id, CompletePutawayTaskDto? dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Completed", Transitions)) return false;

        if (dto?.Lines is { Count: > 0 })
        {
            foreach (var patch in dto.Lines)
            {
                var line = entity.Lines.FirstOrDefault(l => l.Id == patch.LineId);
                if (line == null) return false;
                line.ActualLocationId = patch.ActualLocationId;
            }
        }

        foreach (var line in entity.Lines)
        {
            var loc = line.ActualLocationId ?? line.SuggestedLocationId;
            if (!loc.HasValue || loc == Guid.Empty) return false;
            line.ActualLocationId = loc;
        }

        var stockLines = entity.Lines
            .Select(l => (l.ItemId, l.Quantity, l.ActualLocationId))
            .ToList();
        var received = await _stock.ReceiveStockAsync(tenantId, entity.WarehouseId, stockLines, userId.ToString());
        if (!received) return false;

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

    private async Task<Guid?> SuggestBulkOrPickAsync(Guid warehouseId, Guid tenantId)
    {
        var locs = (await _locations.GetByWarehouseAsync(warehouseId, tenantId)).ToList();
        var prefer = locs.FirstOrDefault(l =>
            string.Equals(l.LocationType, "Bulk", StringComparison.OrdinalIgnoreCase)
            || string.Equals(l.LocationType, "Staging", StringComparison.OrdinalIgnoreCase));
        prefer ??= locs.FirstOrDefault(l =>
            string.Equals(l.LocationType, "Pick", StringComparison.OrdinalIgnoreCase));
        return prefer?.Id ?? locs.FirstOrDefault()?.Id;
    }

    private static PutawayTaskDto Map(PutawayTask e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        TaskNumber = e.TaskNumber,
        WarehouseId = e.WarehouseId,
        ReferenceDocType = e.ReferenceDocType,
        ReferenceDocId = e.ReferenceDocId,
        Status = e.Status,
        Notes = e.Notes,
        CompletedAt = e.CompletedAt,
        CompletedBy = e.CompletedBy,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new PutawayTaskLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            PutawayTaskId = l.PutawayTaskId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            SuggestedLocationId = l.SuggestedLocationId,
            ActualLocationId = l.ActualLocationId,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}

public class PickListService : IPickListService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = new[] { "Picking", "Completed", "Cancelled" },
            ["Picking"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IPickListRepository _repo;
    private readonly IStockBalanceRepository _balances;
    private readonly IWarehouseLocationRepository _locations;
    private readonly IStockService _stock;

    public PickListService(
        IPickListRepository repo,
        IStockBalanceRepository balances,
        IWarehouseLocationRepository locations,
        IStockService stock)
    {
        _repo = repo;
        _balances = balances;
        _locations = locations;
        _stock = stock;
    }

    public async Task<PickListDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<PickListDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreatePickListDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.Lines.Count == 0 || dto.WarehouseId == Guid.Empty) return Guid.Empty;
        var id = Guid.NewGuid();
        var lines = new List<PickListLine>();
        foreach (var l in dto.Lines.Where(x => x.QuantityRequested > 0))
        {
            var from = l.FromLocationId ?? await SuggestPickLocationAsync(l.ItemId, dto.WarehouseId, tenantId);
            lines.Add(new PickListLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PickListId = id,
                ItemId = l.ItemId,
                QuantityRequested = l.QuantityRequested,
                QuantityPicked = 0,
                FromLocationId = from,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            });
        }
        if (lines.Count == 0) return Guid.Empty;

        var entity = new PickList
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = dto.WarehouseId,
            PickNumber = string.IsNullOrWhiteSpace(dto.PickNumber)
                ? $"PICK-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.PickNumber.Trim(),
            ReferenceDocType = string.IsNullOrWhiteSpace(dto.ReferenceDocType) ? "DispatchAdvice" : dto.ReferenceDocType.Trim(),
            ReferenceDocId = dto.ReferenceDocId,
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = lines
        };
        return await _repo.CreateAsync(entity);
    }

    public Task<Guid> CreateFromDispatchAsync(
        Guid dispatchId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, Guid tenantId, Guid userId)
        => CreateAsync(new CreatePickListDto
        {
            WarehouseId = warehouseId,
            ReferenceDocType = "DispatchAdvice",
            ReferenceDocId = dispatchId,
            Notes = $"Created from dispatch {dispatchId:N}",
            Lines = lines.Select(l => new CreatePickListLineDto
            {
                ItemId = l.ItemId,
                QuantityRequested = l.Quantity
            }).ToList()
        }, tenantId, userId);

    public async Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !DocumentWorkflow.CanTransition(entity.Status, "Picking", Transitions)) return false;
        entity.Status = "Picking";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> CompleteAsync(Guid id, CompletePickListDto? dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Completed", Transitions)) return false;

        if (dto?.Lines is { Count: > 0 })
        {
            foreach (var patch in dto.Lines)
            {
                var line = entity.Lines.FirstOrDefault(l => l.Id == patch.LineId);
                if (line == null) return false;
                line.FromLocationId = patch.FromLocationId;
                line.QuantityPicked = patch.QuantityPicked > 0 ? patch.QuantityPicked : line.QuantityRequested;
            }
        }

        foreach (var line in entity.Lines)
        {
            if (!line.FromLocationId.HasValue || line.FromLocationId == Guid.Empty) return false;
            if (line.QuantityPicked <= 0) line.QuantityPicked = line.QuantityRequested;
            if (line.QuantityPicked <= 0) return false;
        }

        var stockLines = entity.Lines
            .Select(l => (l.ItemId, l.QuantityPicked, l.FromLocationId))
            .ToList();
        var issued = await _stock.IssueStockAsync(tenantId, entity.WarehouseId, stockLines, userId.ToString());
        if (!issued) return false;

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

    public async Task<bool> HasCompletedPickForDispatchAsync(Guid dispatchId, Guid tenantId)
    {
        var pick = await _repo.GetCompletedByReferenceAsync("DispatchAdvice", dispatchId, tenantId);
        return pick != null;
    }

    private async Task<Guid?> SuggestPickLocationAsync(Guid itemId, Guid warehouseId, Guid tenantId)
    {
        var locMeta = (await _locations.GetByWarehouseAsync(warehouseId, tenantId))
            .ToDictionary(l => l.Id);
        var balances = (await _balances.GetByItemAndWarehouseAllLocationsAsync(itemId, warehouseId, tenantId))
            .Where(b => b.CurrentQuantity > 0 && b.WarehouseLocationId.HasValue)
            .OrderBy(b =>
            {
                if (!b.WarehouseLocationId.HasValue || !locMeta.TryGetValue(b.WarehouseLocationId.Value, out var loc))
                    return 2;
                return string.Equals(loc.LocationType, "Pick", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            })
            .ThenByDescending(b => b.CurrentQuantity)
            .ToList();
        return balances.FirstOrDefault()?.WarehouseLocationId
               ?? locMeta.Values.FirstOrDefault(l =>
                   string.Equals(l.LocationType, "Pick", StringComparison.OrdinalIgnoreCase))?.Id
               ?? locMeta.Values.FirstOrDefault()?.Id;
    }

    private static PickListDto Map(PickList e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        PickNumber = e.PickNumber,
        WarehouseId = e.WarehouseId,
        ReferenceDocType = e.ReferenceDocType,
        ReferenceDocId = e.ReferenceDocId,
        Status = e.Status,
        Notes = e.Notes,
        CompletedAt = e.CompletedAt,
        CompletedBy = e.CompletedBy,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new PickListLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            PickListId = l.PickListId,
            ItemId = l.ItemId,
            QuantityRequested = l.QuantityRequested,
            QuantityPicked = l.QuantityPicked,
            FromLocationId = l.FromLocationId,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}
