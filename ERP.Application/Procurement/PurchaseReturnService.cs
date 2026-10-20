using ERP.Application.Common;
using ERP.Application.Interfaces;
using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Procurement;

public class PurchaseReturnService : IPurchaseReturnService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pending"] = new[] { "Approved", "Cancelled" },
            ["Approved"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IPurchaseReturnRepository _repository;
    private readonly IStockService _stock;
    private readonly IPurchaseDebitNoteService _debitNotes;

    public PurchaseReturnService(
        IPurchaseReturnRepository repository,
        IStockService stock,
        IPurchaseDebitNoteService debitNotes)
    {
        _repository = repository;
        _stock = stock;
        _debitNotes = debitNotes;
    }

    public async Task<PurchaseReturnDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<PurchaseReturnDto>> GetAllAsync(Guid tenantId)
        => (await _repository.GetAllAsync(tenantId)).Select(Map);

    public async Task<Guid> CreateAsync(CreatePurchaseReturnDto dto, Guid tenantId, Guid userId)
    {
        if (dto.VendorId == Guid.Empty) return Guid.Empty;
        var lines = (dto.Lines ?? []).Where(l => l.ItemId != Guid.Empty && l.Quantity > 0).ToList();
        var total = lines.Count > 0
            ? Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice), 2)
            : dto.TotalAmount;

        var entity = new PurchaseReturn
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturnNumber = string.IsNullOrWhiteSpace(dto.ReturnNumber)
                ? $"PR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
                : dto.ReturnNumber.Trim(),
            ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow.Date : dto.ReturnDate.Date,
            VendorId = dto.VendorId,
            PurchaseOrderId = dto.PurchaseOrderId,
            PurchaseInvoiceId = dto.PurchaseInvoiceId,
            WarehouseId = dto.WarehouseId,
            Reason = dto.Reason ?? string.Empty,
            Status = "Pending",
            TotalAmount = total,
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in lines)
        {
            entity.Lines.Add(new PurchaseReturnLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PurchaseReturnId = entity.Id,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = Math.Round(line.Quantity * line.UnitPrice, 2),
                Reason = line.Reason,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdatePurchaseReturnDto dto, Guid tenantId, Guid userId)
    {
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (!string.Equals(e.Status, "Pending", StringComparison.OrdinalIgnoreCase)) return false;

        if (dto.Reason != null) e.Reason = dto.Reason;
        if (dto.Notes != null) e.Notes = dto.Notes;
        if (dto.WarehouseId.HasValue) e.WarehouseId = dto.WarehouseId;
        if (dto.PurchaseInvoiceId.HasValue) e.PurchaseInvoiceId = dto.PurchaseInvoiceId;
        if (dto.TotalAmount.HasValue && (dto.Lines == null || dto.Lines.Count == 0))
            e.TotalAmount = dto.TotalAmount.Value;

        if (dto.Lines != null)
        {
            e.Lines.Clear();
            foreach (var line in dto.Lines.Where(l => l.ItemId != Guid.Empty && l.Quantity > 0))
            {
                e.Lines.Add(new PurchaseReturnLine
                {
                    Id = Guid.NewGuid(),
                    TenantId = e.TenantId,
                    PurchaseReturnId = e.Id,
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = Math.Round(line.Quantity * line.UnitPrice, 2),
                    Reason = line.Reason,
                    CreatedAt = DateTime.UtcNow
                });
            }
            e.TotalAmount = Math.Round(e.Lines.Sum(l => l.LineTotal), 2);
        }

        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var e = await _repository.GetByIdAsync(id, tenantId);
        if (e == null) return false;
        if (!string.Equals(e.Status, "Pending", StringComparison.OrdinalIgnoreCase)) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, userId, "Approved");

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, userId, "Cancelled");

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var e = await _repository.GetByIdAsync(id, tenantId);
        if (e == null) return false;
        if (!DocumentWorkflow.CanTransition(e.Status, "Completed", Transitions)) return false;
        if (e.TotalAmount <= 0 && e.Lines.Count == 0) return false;

        if (!e.DebitNoteId.HasValue)
        {
            // Monetary return → PurchaseDebitNote against an approved PI (AP/GL/stock via DN post)
            if (!e.PurchaseInvoiceId.HasValue || e.PurchaseInvoiceId == Guid.Empty)
                return false;

            var lineIndex = 1;
            var dnId = await _debitNotes.CreateAsync(new CreatePurchaseDebitNoteDto
            {
                DebitNoteNumber = $"DN-{e.ReturnNumber}",
                DebitNoteDate = e.ReturnDate,
                VendorId = e.VendorId,
                OriginalPurchaseInvoiceId = e.PurchaseInvoiceId.Value,
                PurchaseReturnId = e.Id,
                WarehouseId = e.WarehouseId,
                Reason = e.Reason,
                Notes = string.IsNullOrWhiteSpace(e.Notes) ? $"PurchaseReturn:{e.Id}" : e.Notes,
                Lines = e.Lines.Count > 0
                    ? e.Lines.Select(l => new CreatePurchaseDebitNoteLineDto
                    {
                        ItemId = l.ItemId,
                        Description = string.IsNullOrWhiteSpace(l.Reason) ? e.Reason : l.Reason,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        TaxRate = 0,
                        LineNumber = lineIndex++
                    }).ToList()
                    : [
                        new CreatePurchaseDebitNoteLineDto
                        {
                            Description = e.Reason,
                            Quantity = 1,
                            UnitPrice = e.TotalAmount,
                            TaxRate = 0,
                            LineNumber = 1
                        }
                    ]
            }, e.TenantId, userId);

            if (dnId == Guid.Empty) return false;
            if (!await _debitNotes.PostAsync(dnId, e.TenantId, userId))
                return false;

            e.DebitNoteId = dnId;
        }
        else if (e.WarehouseId.HasValue && e.WarehouseId != Guid.Empty && e.Lines.Count > 0)
        {
            // Already linked DN (posted elsewhere): stock-only if warehouse set and DN did not issue
            var dn = await _debitNotes.GetByIdAsync(e.DebitNoteId.Value, e.TenantId);
            if (dn == null || !dn.StockIssued)
            {
                var stockLines = e.Lines.Select(l => (l.ItemId, l.Quantity));
                var issued = await _stock.IssueStockAsync(
                    e.TenantId, e.WarehouseId.Value, stockLines, userId.ToString());
                if (!issued) return false;
            }
        }

        e.Status = "Completed";
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(e);
    }

    private async Task<bool> TransitionAsync(Guid id, Guid tenantId, Guid userId, string toStatus)
    {
        var e = await _repository.GetByIdAsync(id, tenantId);
        if (e == null) return false;
        if (!DocumentWorkflow.CanTransition(e.Status, toStatus, Transitions)) return false;
        e.Status = toStatus;
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(e);
    }

    private static PurchaseReturnDto Map(PurchaseReturn e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ReturnNumber = e.ReturnNumber,
        ReturnDate = e.ReturnDate,
        VendorId = e.VendorId,
        VendorName = e.Vendor?.VendorName,
        PurchaseOrderId = e.PurchaseOrderId,
        PurchaseInvoiceId = e.PurchaseInvoiceId,
        WarehouseId = e.WarehouseId,
        DebitNoteId = e.DebitNoteId,
        Reason = e.Reason,
        Status = e.Status,
        TotalAmount = e.TotalAmount,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines?.Select(l => new PurchaseReturnLineDto
        {
            Id = l.Id,
            PurchaseReturnId = l.PurchaseReturnId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal,
            Reason = l.Reason
        }).ToList() ?? new List<PurchaseReturnLineDto>()
    };
}
