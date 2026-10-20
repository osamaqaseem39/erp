using ERP.Application.Common;
using ERP.Application.Interfaces;
using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Procurement;

public class GoodsReceiptNoteService : IGoodsReceiptNoteService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Posted", "Cancelled" },
            ["Verified"] = new[] { "Posted", "Cancelled" },
        };

    private readonly IGoodsReceiptNoteRepository _repository;
    private readonly IGoodsReceiptNoteDetailRepository _detailRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseOrderDetailRepository _purchaseOrderDetailRepository;
    private readonly IStockService _stockService;

    public GoodsReceiptNoteService(
        IGoodsReceiptNoteRepository repository,
        IGoodsReceiptNoteDetailRepository detailRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseOrderDetailRepository purchaseOrderDetailRepository,
        IStockService stockService)
    {
        _repository = repository;
        _detailRepository = detailRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderDetailRepository = purchaseOrderDetailRepository;
        _stockService = stockService;
    }

    public async Task<GoodsReceiptNoteDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;
        var details = (await _detailRepository.GetByGrnIdAsync(id, tenantId)).ToList();
        entity.Details = details;
        return MapToDto(entity, true);
    }

    public async Task<IEnumerable<GoodsReceiptNoteDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(e => MapToDto(e, false));

    public async Task<Guid> CreateAsync(CreateGoodsReceiptNoteDto dto, Guid tenantId, Guid userId)
    {
        var poId = dto.POId ?? dto.PurchaseOrderId ?? Guid.Empty;
        var entity = new GoodsReceiptNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            POId = poId,
            VendorId = dto.VendorId,
            GRNNumber = string.IsNullOrWhiteSpace(dto.GRNNumber)
                ? $"GRN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
                : dto.GRNNumber,
            ReceivedDate = dto.ReceiptDate ?? DateTime.UtcNow,
            WarehouseId = dto.WarehouseId ?? Guid.Empty,
            ReceivedByUserId = userId,
            Status = "Draft",
            Remarks = dto.Remarks ?? dto.Notes ?? string.Empty,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        var id = await _repository.CreateAsync(entity);
        await SaveDetailsAsync(id, tenantId, userId, dto.Details);
        return id;
    }

    public async Task<bool> UpdateAsync(UpdateGoodsReceiptNoteDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null || IsImmutable(entity.Status)) return false;

        if (dto.CompanyId.HasValue) entity.CompanyId = dto.CompanyId.Value;
        if (dto.VendorId.HasValue) entity.VendorId = dto.VendorId.Value;
        var poId = dto.POId ?? dto.PurchaseOrderId;
        if (poId.HasValue) entity.POId = poId.Value;
        if (dto.WarehouseId.HasValue) entity.WarehouseId = dto.WarehouseId.Value;
        if (!string.IsNullOrWhiteSpace(dto.GRNNumber)) entity.GRNNumber = dto.GRNNumber;
        if (dto.ReceiptDate.HasValue) entity.ReceivedDate = dto.ReceiptDate.Value;
        // ERP-FIX-5: status only via approve (post) / reject
        if (dto.Remarks != null || dto.Notes != null)
            entity.Remarks = dto.Remarks ?? dto.Notes ?? entity.Remarks;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _detailRepository.DeleteByGrnIdAsync(entity.Id, tenantId);
        await SaveDetailsAsync(entity.Id, tenantId, userId, dto.Details);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null || IsImmutable(entity.Status)) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        await _detailRepository.DeleteByGrnIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<GoodsReceiptNoteDto>> GetByPurchaseOrderAsync(Guid purchaseOrderId, Guid tenantId) =>
        (await _repository.GetByPurchaseOrderAsync(purchaseOrderId, tenantId)).Select(e => MapToDto(e, false));

    public async Task<IEnumerable<GoodsReceiptNoteDto>> GetByStatusAsync(string status, Guid tenantId) =>
        (await _repository.GetByStatusAsync(status, tenantId)).Select(e => MapToDto(e, false));

    public async Task<IEnumerable<GoodsReceiptNoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId) =>
        (await _repository.GetByDateRangeAsync(startDate, endDate, tenantId)).Select(e => MapToDto(e, false));

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Posted", Transitions))
            return false;

        var details = (await _detailRepository.GetByGrnIdAsync(id, tenantId)).ToList();
        if (details.Count == 0) return false;
        if (entity.WarehouseId == Guid.Empty) return false;

        var stockLines = details
            .Where(d => d.QuantityReceived > 0)
            .Select(d => (d.ItemId, d.QuantityReceived));
        var received = await _stockService.ReceiveStockAsync(
            tenantId, entity.WarehouseId, stockLines, userId.ToString());
        if (!received) return false;

        if (entity.POId != Guid.Empty)
            await ApplyReceiptToPurchaseOrderAsync(entity.POId, tenantId, details, userId);

        entity.Status = "Posted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions))
            return false;
        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task ApplyReceiptToPurchaseOrderAsync(
        Guid poId,
        Guid tenantId,
        IReadOnlyList<GoodsReceiptNoteDetail> grnDetails,
        Guid userId)
    {
        var po = await _purchaseOrderRepository.GetByIdAsync(poId, tenantId);
        if (po == null) return;

        var poLines = (await _purchaseOrderDetailRepository.GetByPurchaseOrderIdAsync(poId, tenantId)).ToList();
        foreach (var grnLine in grnDetails.Where(d => d.QuantityReceived > 0))
        {
            var remaining = grnLine.QuantityReceived;
            foreach (var poLine in poLines.Where(l => l.ItemId == grnLine.ItemId))
            {
                if (remaining <= 0) break;
                var open = Math.Max(0, poLine.QuantityOrdered - poLine.ReceivedQuantity);
                if (open <= 0) continue;
                var apply = Math.Min(remaining, open);
                poLine.ReceivedQuantity += apply;
                await _purchaseOrderDetailRepository.UpdateAsync(poLine);
                remaining -= apply;
            }
        }

        var refreshed = (await _purchaseOrderDetailRepository.GetByPurchaseOrderIdAsync(poId, tenantId)).ToList();
        if (refreshed.Count > 0)
        {
            var allReceived = refreshed.All(l => l.ReceivedQuantity >= l.QuantityOrdered);
            var anyReceived = refreshed.Any(l => l.ReceivedQuantity > 0);
            var next = allReceived ? "Completed" : anyReceived ? "PartiallyReceived" : null;
            if (next != null
                && !string.Equals(po.Status, next, StringComparison.OrdinalIgnoreCase)
                && DocumentWorkflow.CanTransition(po.Status, next, PurchaseOrderService.ReceiptTransitions))
                po.Status = next;
        }

        po.UpdatedBy = userId.ToString();
        po.UpdatedAt = DateTime.UtcNow;
        await _purchaseOrderRepository.UpdateAsync(po);
    }

    private async Task SaveDetailsAsync(Guid grnId, Guid tenantId, Guid userId, IEnumerable<CreateGoodsReceiptNoteDetailDto> details)
    {
        foreach (var line in details)
        {
            var qty = line.ReceivedQuantity ?? line.AcceptedQuantity ?? 0;
            var total = Math.Round(qty * line.UnitPrice + line.TaxAmount, 2);
            await _detailRepository.CreateAsync(new GoodsReceiptNoteDetail
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                GRNId = grnId,
                ItemId = line.ItemId,
                QuantityReceived = qty,
                UnitPrice = line.UnitPrice,
                TaxAmount = line.TaxAmount,
                TotalAmount = total,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static bool IsImmutable(string status) =>
        string.Equals(status, "Posted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    private static GoodsReceiptNoteDto MapToDto(GoodsReceiptNote entity, bool includeDetails)
    {
        var details = includeDetails && entity.Details != null
            ? entity.Details.Select(d => new GoodsReceiptNoteDetailDto
            {
                Id = d.Id,
                TenantId = d.TenantId,
                GRNId = d.GRNId,
                ItemId = d.ItemId,
                QuantityReceived = d.QuantityReceived,
                ReceivedQuantity = d.QuantityReceived,
                UnitPrice = d.UnitPrice,
                TaxAmount = d.TaxAmount,
                TotalAmount = d.TotalAmount,
                BatchSerialId = d.BatchSerialId,
                CreatedAt = d.CreatedAt,
                CreatedBy = d.CreatedBy
            }).ToList()
            : null;

        return new GoodsReceiptNoteDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            GRNNumber = entity.GRNNumber,
            VendorId = entity.VendorId,
            PurchaseOrderId = entity.POId,
            POId = entity.POId,
            ReceiptDate = entity.ReceivedDate,
            ReceivedDate = entity.ReceivedDate,
            WarehouseId = entity.WarehouseId,
            ReceivedByUserId = entity.ReceivedByUserId,
            Status = entity.Status,
            Remarks = entity.Remarks,
            Notes = entity.Remarks,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive,
            Details = details,
            GRNDetails = details
        };
    }
}
