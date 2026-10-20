using ERP.Application.Common;
using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Inventory;

public class StockTransferService : IStockTransferService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = ["InTransit", "Cancelled"],
            ["InTransit"] = ["Completed", "Cancelled"],
            ["Completed"] = [],
            ["Cancelled"] = []
        };

    private readonly IStockTransferRepository _repository;
    private readonly IStockService _stock;

    public StockTransferService(IStockTransferRepository repository, IStockService stock)
    {
        _repository = repository;
        _stock = stock;
    }

    public async Task<StockTransferDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<StockTransferDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreateStockTransferDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.FromWarehouseId == Guid.Empty || dto.ToWarehouseId == Guid.Empty)
            return Guid.Empty;
        if (dto.FromWarehouseId == dto.ToWarehouseId)
            return Guid.Empty;
        if (dto.Details == null || dto.Details.Count == 0)
            return Guid.Empty;
        if (dto.Details.Any(d => d.ItemId == Guid.Empty || d.Quantity <= 0))
            return Guid.Empty;

        var id = Guid.NewGuid();
        var entity = new StockTransfer
        {
            Id = id,
            TenantId = tenantId,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TransferDate = dto.TransferDate == default ? DateTime.UtcNow.Date : dto.TransferDate,
            Status = "Draft",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            StockTransferDetails = dto.Details.Select(d => new StockTransferDetail
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TransferId = id,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                FromLocationId = d.FromLocationId,
                ToLocationId = d.ToLocationId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList()
        };

        var created = await _repository.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateStockTransferDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        if (dto.FromWarehouseId == Guid.Empty || dto.ToWarehouseId == Guid.Empty)
            return false;
        if (dto.FromWarehouseId == dto.ToWarehouseId)
            return false;
        if (dto.Details == null || dto.Details.Count == 0)
            return false;

        entity.FromWarehouseId = dto.FromWarehouseId;
        entity.ToWarehouseId = dto.ToWarehouseId;
        entity.TransferDate = dto.TransferDate == default ? entity.TransferDate : dto.TransferDate;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Status = "Draft";
        await _repository.UpdateAsync(entity);

        var lines = new List<StockTransferDetail>();
        foreach (var d in dto.Details)
        {
            if (d.ItemId == Guid.Empty || d.Quantity <= 0) return false;
            lines.Add(new StockTransferDetail
            {
                Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
                TenantId = tenantId,
                TransferId = entity.Id,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                FromLocationId = d.FromLocationId,
                ToLocationId = d.ToLocationId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _repository.ReplaceDetailsAsync(entity.Id, tenantId, lines);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId, Guid tenantId) =>
        (await _repository.GetByWarehouseAsync(warehouseId, tenantId))
            .Where(e => e.FromWarehouseId == warehouseId)
            .Select(Map);

    public async Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId, Guid tenantId) =>
        (await _repository.GetByWarehouseAsync(warehouseId, tenantId))
            .Where(e => e.ToWarehouseId == warehouseId)
            .Select(Map);

    public async Task<IEnumerable<StockTransferDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId) =>
        (await _repository.GetByWarehouseAsync(warehouseId, tenantId)).Select(Map);

    public async Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status, Guid tenantId) =>
        (await _repository.GetByStatusAsync(status, tenantId)).Select(Map);

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "InTransit", Transitions))
            return false;
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId))
            return false;
        var details = entity.StockTransferDetails?.ToList() ?? [];
        if (details.Count == 0) return false;

        if (await _stock.IsBinRequiredAsync()
            && details.Any(d => !d.FromLocationId.HasValue || d.FromLocationId == Guid.Empty))
            return false;

        var issueLines = details
            .Select(d => (d.ItemId, d.Quantity, d.FromLocationId))
            .ToList();
        var issued = await _stock.IssueStockAsync(
            tenantId, entity.FromWarehouseId, issueLines, userId.ToString());
        if (!issued) return false;

        entity.Status = "InTransit";
        entity.ApprovedBy = userId.ToString();
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Completed", Transitions))
            return false;

        var details = entity.StockTransferDetails?.ToList() ?? [];
        if (details.Count == 0) return false;

        if (await _stock.IsBinRequiredAsync()
            && details.Any(d => !d.ToLocationId.HasValue || d.ToLocationId == Guid.Empty))
            return false;

        var receiveLines = details
            .Select(d => (d.ItemId, d.Quantity, d.ToLocationId))
            .ToList();
        var received = await _stock.ReceiveStockAsync(
            tenantId, entity.ToWarehouseId, receiveLines, userId.ToString());
        if (!received) return false;

        entity.Status = "Completed";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions))
            return false;

        // Reverse in-transit: put qty back at source.
        if (string.Equals(DocumentWorkflow.Normalize(entity.Status), "InTransit", StringComparison.OrdinalIgnoreCase))
        {
            var details = entity.StockTransferDetails?.ToList() ?? [];
            if (details.Count == 0) return false;
            var reverse = details
                .Select(d => (d.ItemId, d.Quantity, d.FromLocationId))
                .ToList();
            var restored = await _stock.ReceiveStockAsync(
                tenantId, entity.FromWarehouseId, reverse, userId.ToString());
            if (!restored) return false;
        }

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    private static StockTransferDto Map(StockTransfer e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        TransferNumber = e.Id.ToString("N")[..8].ToUpperInvariant(),
        FromWarehouseId = e.FromWarehouseId,
        FromWarehouseName = e.FromWarehouse?.WarehouseName,
        ToWarehouseId = e.ToWarehouseId,
        ToWarehouseName = e.ToWarehouse?.WarehouseName,
        TransferDate = e.TransferDate,
        Status = e.Status,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        IsActive = e.IsActive,
        Details = (e.StockTransferDetails ?? []).Select(d => new StockTransferDetailDto
        {
            Id = d.Id,
            StockTransferId = e.Id,
            ItemId = d.ItemId,
            ItemName = d.Item?.ItemName,
            Quantity = d.Quantity,
            FromLocationId = d.FromLocationId,
            ToLocationId = d.ToLocationId
        }).ToList()
    };
}
