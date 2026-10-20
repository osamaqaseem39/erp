using ERP.Application.Common;
using ERP.Application.Interfaces.Inventory;
using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Shared.DTOs.Sales;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Sales;

public class DispatchAdviceService : IDispatchAdviceService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Approved", "Cancelled" },
            ["Approved"] = new[] { "Dispatched", "Cancelled" },
            ["Dispatched"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IDispatchAdviceRepository _repo;
    private readonly IDocumentNumberService _numbers;
    private readonly IStockService _stock;
    private readonly IPickListService? _picks;
    private readonly IPackOrderService? _packs;
    private readonly IShipmentService? _shipments;

    public DispatchAdviceService(
        IDispatchAdviceRepository repo,
        IDocumentNumberService numbers,
        IStockService stock,
        IPickListService? picks = null,
        IPackOrderService? packs = null,
        IShipmentService? shipments = null)
    {
        _repo = repo;
        _numbers = numbers;
        _stock = stock;
        _picks = picks;
        _packs = packs;
        _shipments = shipments;
    }

    public async Task<DispatchAdviceDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repo.GetByIdAsync(id, tenantId, includeLines: true);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<DispatchAdviceDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repo.GetAllAsync(tenantId)).Select(MapHeader);
    }

    public async Task<Guid> CreateAsync(CreateDispatchAdviceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.CustomerId == Guid.Empty) return Guid.Empty;
        var lines = (dto.Lines ?? []).Where(l => l.ItemId != Guid.Empty && l.Quantity > 0).ToList();
        if (lines.Count == 0) return Guid.Empty;

        var entity = new DispatchAdvice
        {
            TenantId = tenantId,
            AdviceNumber = await _numbers.NextAsync("sales.dispatchAdvice"),
            SalesOrderId = dto.SalesOrderId,
            CustomerId = dto.CustomerId,
            WarehouseId = dto.WarehouseId,
            AdviceDate = dto.AdviceDate?.Date ?? DateTime.UtcNow.Date,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            Lines = lines.Select(l => new DispatchAdviceLine
            {
                TenantId = tenantId,
                SalesOrderLineId = l.SalesOrderLineId,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                Notes = l.Notes
            }).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateDispatchAdviceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.SalesOrderId = dto.SalesOrderId;
        entity.CustomerId = dto.CustomerId;
        entity.WarehouseId = dto.WarehouseId;
        entity.AdviceDate = dto.AdviceDate?.Date ?? entity.AdviceDate;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        if (!await _repo.UpdateAsync(entity)) return false;

        var lines = (dto.Lines ?? []).Where(l => l.ItemId != Guid.Empty && l.Quantity > 0)
            .Select(l => new DispatchAdviceLine
            {
                TenantId = tenantId,
                SalesOrderLineId = l.SalesOrderLineId,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                Notes = l.Notes
            });
        await _repo.ReplaceLinesAsync(entity.Id, tenantId, lines);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repo.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Approved", Transitions)) return false;
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId)) return false;
        entity.Status = "Approved";
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> DispatchAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId, includeLines: true);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Dispatched", Transitions)) return false;

        var issueLines = entity.Lines
            .Where(l => l.ItemId != Guid.Empty && l.Quantity > 0)
            .Select(l => (l.ItemId, l.Quantity))
            .ToList();
        if (issueLines.Count == 0) return false;

        // When bins are required: pick issued stock; pack+ship must also be done (no second debit).
        if (await _stock.IsBinRequiredAsync())
        {
            if (_picks == null) return false;
            if (!await _picks.HasCompletedPickForDispatchAsync(entity.Id, tenantId))
                return false;
            if (_packs != null && !await _packs.HasPackedForDispatchAsync(entity.Id, tenantId))
                return false;
            if (_shipments != null && !await _shipments.HasShippedForDispatchAsync(entity.Id, tenantId))
                return false;
        }
        else
        {
            var issued = entity.WarehouseId.HasValue && entity.WarehouseId != Guid.Empty
                ? await _stock.IssueStockAsync(tenantId, entity.WarehouseId.Value, issueLines, userId.ToString())
                : await _stock.IssueStockAnyWarehouseAsync(tenantId, issueLines, userId.ToString());
            if (!issued) return false;
        }

        entity.Status = "Dispatched";
        entity.DispatchedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    public async Task<Guid> CreatePickAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (_picks == null || tenantId == Guid.Empty) return Guid.Empty;
        var entity = await _repo.GetByIdAsync(id, tenantId, includeLines: true);
        if (entity == null) return Guid.Empty;
        if (!entity.WarehouseId.HasValue || entity.WarehouseId == Guid.Empty) return Guid.Empty;
        var lines = entity.Lines
            .Where(l => l.ItemId != Guid.Empty && l.Quantity > 0)
            .Select(l => (l.ItemId, l.Quantity));
        return await _picks.CreateFromDispatchAsync(
            entity.Id, entity.WarehouseId.Value, lines, tenantId, userId);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions)) return false;
        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    private static DispatchAdviceDto Map(DispatchAdvice e)
    {
        var dto = MapHeader(e);
        dto.Lines = e.Lines.Select(l => new DispatchAdviceLineDto
        {
            Id = l.Id,
            SalesOrderLineId = l.SalesOrderLineId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            Notes = l.Notes
        }).ToList();
        return dto;
    }

    private static DispatchAdviceDto MapHeader(DispatchAdvice e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        AdviceNumber = e.AdviceNumber,
        SalesOrderId = e.SalesOrderId,
        CustomerId = e.CustomerId,
        WarehouseId = e.WarehouseId,
        AdviceDate = e.AdviceDate,
        Status = e.Status,
        DispatchedAt = e.DispatchedAt,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
