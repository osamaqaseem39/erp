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
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList()
        };

        var created = await _repository.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateStockTransferDto dto, Guid tenantId, Guid userId)
    {
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
        // ERP-FIX-5: status only via approve/complete/cancel
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
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _repository.ReplaceDetailsAsync(entity.Id, tenantId, lines);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
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
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "InTransit", Transitions))
            return false;
        // ERP-FIX-6: creator cannot approve
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId))
            return false;
        if (entity.StockTransferDetails == null || entity.StockTransferDetails.Count == 0)
            return false;

        entity.Status = "InTransit";
        entity.ApprovedBy = userId.ToString();
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Completed", Transitions))
            return false;

        var details = entity.StockTransferDetails?.ToList() ?? [];
        if (details.Count == 0) return false;

        var moved = await _stock.TransferStockAsync(new StockTransferDto
        {
            TenantId = tenantId,
            FromWarehouseId = entity.FromWarehouseId,
            ToWarehouseId = entity.ToWarehouseId,
            TransferDate = entity.TransferDate,
            Status = entity.Status,
            CreatedBy = userId.ToString(),
            UpdatedBy = userId.ToString(),
            Details = details.Select(d => new StockTransferDetailDto
            {
                Id = d.Id,
                StockTransferId = entity.Id,
                ItemId = d.ItemId,
                Quantity = d.Quantity
            }).ToList()
        });
        if (!moved) return false;

        entity.Status = "Completed";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions))
            return false;

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
            Quantity = d.Quantity
        }).ToList()
    };
}
