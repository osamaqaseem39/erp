using ERP.Application.Common;
using ERP.Application.Interfaces;
using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Inventory;

public class StockAdjustmentService : IStockAdjustmentService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = ["Posted", "Cancelled"],
            ["Posted"] = [],
            ["Cancelled"] = []
        };

    private readonly IStockAdjustmentRepository _repository;
    private readonly IStockService _stock;

    public StockAdjustmentService(IStockAdjustmentRepository repository, IStockService stock)
    {
        _repository = repository;
        _stock = stock;
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<StockAdjustmentDto>> GetAllAsync(Guid tenantId)
        => (await _repository.GetAllAsync(tenantId)).Select(Map);

    public async Task<IEnumerable<StockAdjustmentDto>> GetByItemAsync(Guid itemId, Guid tenantId)
        => (await _repository.GetByItemAsync(itemId, tenantId)).Select(Map);

    public async Task<IEnumerable<StockAdjustmentDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
        => (await _repository.GetByWarehouseAsync(warehouseId, tenantId)).Select(Map);

    public async Task<IEnumerable<StockAdjustmentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
        => (await _repository.GetByDateRangeAsync(tenantId, startDate, endDate)).Select(Map);

    public async Task<Guid> CreateAsync(CreateStockAdjustmentDto dto, Guid tenantId, Guid userId)
    {
        if (dto.WarehouseId == Guid.Empty || dto.Details == null || dto.Details.Count == 0)
            return Guid.Empty;

        var id = Guid.NewGuid();
        var entity = new StockAdjustment
        {
            Id = id,
            TenantId = tenantId,
            AdjustmentNumber = string.IsNullOrWhiteSpace(dto.AdjustmentNumber)
                ? $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.AdjustmentNumber.Trim(),
            AdjustmentDate = dto.AdjustmentDate == default ? DateTime.UtcNow.Date : dto.AdjustmentDate,
            WarehouseId = dto.WarehouseId,
            AdjustmentType = string.IsNullOrWhiteSpace(dto.AdjustmentType) ? "Physical" : dto.AdjustmentType,
            Status = "Draft",
            Reference = dto.Reference,
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Details.Select(d => new StockAdjustmentLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StockAdjustmentId = id,
                ItemId = d.ItemId,
                QuantityOnHand = d.QuantityOnHand,
                QuantityCounted = d.QuantityCounted,
                QuantityDifference = d.QuantityCounted - d.QuantityOnHand,
                Notes = d.Notes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList()
        };

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateStockAdjustmentDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        if (dto.Details == null || dto.Details.Count == 0) return false;

        entity.AdjustmentNumber = string.IsNullOrWhiteSpace(dto.AdjustmentNumber)
            ? entity.AdjustmentNumber
            : dto.AdjustmentNumber.Trim();
        entity.AdjustmentDate = dto.AdjustmentDate == default ? entity.AdjustmentDate : dto.AdjustmentDate;
        entity.WarehouseId = dto.WarehouseId == Guid.Empty ? entity.WarehouseId : dto.WarehouseId;
        entity.AdjustmentType = string.IsNullOrWhiteSpace(dto.AdjustmentType) ? entity.AdjustmentType : dto.AdjustmentType;
        entity.Reference = dto.Reference;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        entity.Lines.Clear();
        foreach (var d in dto.Details)
        {
            entity.Lines.Add(new StockAdjustmentLine
            {
                Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
                TenantId = tenantId,
                StockAdjustmentId = entity.Id,
                ItemId = d.ItemId,
                QuantityOnHand = d.QuantityOnHand,
                QuantityCounted = d.QuantityCounted,
                QuantityDifference = d.QuantityCounted - d.QuantityOnHand,
                Notes = d.Notes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        return await _repository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repository.DeleteAsync(id, tenantId);

    public async Task<bool> PostAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Posted", Transitions)) return false;
        if (entity.Lines.Count == 0) return false;

        var stockDto = new StockAdjustmentDto
        {
            TenantId = tenantId,
            WarehouseId = entity.WarehouseId,
            CreatedBy = userId.ToString(),
            UpdatedBy = userId.ToString(),
            Details = entity.Lines.Select(l => new StockAdjustmentDetailDto
            {
                ItemId = l.ItemId,
                QuantityOnHand = l.QuantityOnHand,
                QuantityCounted = l.QuantityCounted,
                QuantityDifference = l.QuantityDifference
            }).ToList()
        };

        if (!await _stock.AdjustStockAsync(stockDto)) return false;

        entity.Status = "Posted";
        entity.PostedBy = userId.ToString();
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions)) return false;
        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private static StockAdjustmentDto Map(StockAdjustment e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        AdjustmentNumber = e.AdjustmentNumber,
        AdjustmentDate = e.AdjustmentDate,
        WarehouseId = e.WarehouseId,
        WarehouseName = e.Warehouse?.WarehouseName ?? e.Warehouse?.Name,
        AdjustmentType = e.AdjustmentType,
        Status = e.Status,
        Reference = e.Reference,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Details = e.Lines.Select(l => new StockAdjustmentDetailDto
        {
            Id = l.Id,
            StockAdjustmentId = l.StockAdjustmentId,
            ItemId = l.ItemId,
            ItemName = l.Item?.ItemName,
            QuantityOnHand = l.QuantityOnHand,
            QuantityCounted = l.QuantityCounted,
            QuantityDifference = l.QuantityDifference,
            Notes = l.Notes
        }).ToList()
    };
}
