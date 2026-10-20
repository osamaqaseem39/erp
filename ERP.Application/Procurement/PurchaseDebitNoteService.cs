using ERP.Application.Common;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Procurement;

public class PurchaseDebitNoteService : IPurchaseDebitNoteService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Posted" },
            ["Posted"] = new[] { "Voided" },
        };

    private readonly IPurchaseDebitNoteRepository _repository;
    private readonly IPurchaseDebitNoteLineRepository _lineRepository;
    private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
    private readonly IBillService _bills;
    private readonly IAccountingService? _accounting;
    private readonly ERPDbContext? _db;
    private readonly IStockService? _stock;

    public PurchaseDebitNoteService(
        IPurchaseDebitNoteRepository repository,
        IPurchaseDebitNoteLineRepository lineRepository,
        IPurchaseInvoiceRepository purchaseInvoiceRepository,
        IBillService bills,
        IAccountingService? accounting = null,
        ERPDbContext? db = null,
        IStockService? stock = null)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _purchaseInvoiceRepository = purchaseInvoiceRepository;
        _bills = bills;
        _accounting = accounting;
        _db = db;
        _stock = stock;
    }

    public async Task<PurchaseDebitNoteDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;

        var dto = MapToDto(entity);
        var lines = await _lineRepository.GetByDebitNoteIdAsync(id, tenantId);
        dto.Lines = lines.Select(MapLineToDto).ToList();
        return dto;
    }

    public async Task<IEnumerable<PurchaseDebitNoteDto>> GetAllAsync(Guid tenantId)
        => (await _repository.GetAllAsync(tenantId)).Select(MapToDto);

    public async Task<IEnumerable<PurchaseDebitNoteDto>> GetByVendorAsync(Guid vendorId, Guid tenantId)
        => (await _repository.GetByVendorAsync(vendorId, tenantId)).Select(MapToDto);

    public async Task<IEnumerable<PurchaseDebitNoteDto>> GetByStatusAsync(string status, Guid tenantId)
        => (await _repository.GetByStatusAsync(status, tenantId)).Select(MapToDto);

    public async Task<Guid> CreateAsync(CreatePurchaseDebitNoteDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        ApplyLineTotals(entity, dto.Lines);

        var created = await _repository.AddAsync(entity);
        await SaveLinesAsync(created.Id, tenantId, userId, dto.Lines);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdatePurchaseDebitNoteDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (IsImmutable(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity, userId);
        entity.Status = previousStatus;
        ApplyLineTotals(entity, dto.Lines.Select(ToCreateLine));

        await _lineRepository.DeleteByDebitNoteIdAsync(entity.Id, tenantId);
        await SaveLinesAsync(entity.Id, tenantId, userId, dto.Lines.Select(ToCreateLine));
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        await _lineRepository.DeleteByDebitNoteIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> PostAsync(Guid debitNoteId, Guid tenantId, Guid postedBy)
    {
        var entity = await _repository.GetByIdAsync(debitNoteId, tenantId);
        if (entity == null) return false;

        if (string.Equals(DocumentWorkflow.Normalize(entity.Status), "Posted", StringComparison.OrdinalIgnoreCase)
            && entity.AppliedFinanceBillId.HasValue)
            return true;

        if (!DocumentWorkflow.CanTransition(entity.Status, "Posted", Transitions))
            return false;

        if (entity.TotalAmount <= 0) return false;

        var purchaseInvoice = await _purchaseInvoiceRepository.GetByIdAsync(entity.OriginalPurchaseInvoiceId, tenantId);
        if (purchaseInvoice == null) return false;
        if (!string.Equals(purchaseInvoice.Status, "Approved", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(purchaseInvoice.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            return false;
        if (purchaseInvoice.VendorId != entity.VendorId)
            return false;

        var ap = await ResolveApBillAsync(purchaseInvoice, tenantId);
        if (ap == null || ap.Id == Guid.Empty) return false;

        var billId = ap.Id;
        if (entity.TotalAmount > ap.Balance)
            return false;

        var applied = await _bills.ApplyCreditToBillAsync(billId, entity.TotalAmount, tenantId);
        if (!applied) return false;

        Guid? journalId = null;
        if (!entity.PostedJournalId.HasValue)
        {
            journalId = await TryPostGlJournalAsync(entity, tenantId, postedBy);
            if (journalId == Guid.Empty)
            {
                // Defaults missing → AP-only post
                journalId = null;
            }
            else if (journalId == null)
            {
                await _bills.UnapplyCreditFromBillAsync(billId, entity.TotalAmount, tenantId);
                return false;
            }
        }
        else
        {
            journalId = entity.PostedJournalId;
        }

        var stockIssued = entity.StockIssued;
        if (!stockIssued
            && entity.WarehouseId.HasValue
            && entity.WarehouseId != Guid.Empty
            && _stock != null)
        {
            var lines = await _lineRepository.GetByDebitNoteIdAsync(entity.Id, tenantId);
            var stockLines = lines
                .Where(l => l.ItemId.HasValue && l.ItemId != Guid.Empty && l.Quantity > 0)
                .Select(l => (l.ItemId!.Value, l.Quantity))
                .ToList();
            if (stockLines.Count > 0)
            {
                var issued = await _stock.IssueStockAsync(
                    tenantId, entity.WarehouseId.Value, stockLines, postedBy.ToString());
                if (!issued)
                {
                    await RollbackApAndGlAsync(billId, entity.TotalAmount, journalId, tenantId, postedBy);
                    return false;
                }
                stockIssued = true;
            }
        }

        entity.Status = "Posted";
        entity.PostedAt = DateTime.UtcNow;
        entity.PostedBy = postedBy;
        entity.AppliedFinanceBillId = billId;
        entity.PostedJournalId = journalId;
        entity.StockIssued = stockIssued;
        entity.UpdatedBy = postedBy.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> VoidAsync(Guid debitNoteId, VoidPurchaseDebitNoteDto dto, Guid tenantId, Guid voidedBy)
    {
        var entity = await _repository.GetByIdAsync(debitNoteId, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Voided", Transitions))
            return false;

        if (entity.StockIssued
            && entity.WarehouseId.HasValue
            && entity.WarehouseId != Guid.Empty
            && _stock != null)
        {
            var lines = await _lineRepository.GetByDebitNoteIdAsync(entity.Id, tenantId);
            var stockLines = lines
                .Where(l => l.ItemId.HasValue && l.ItemId != Guid.Empty && l.Quantity > 0)
                .Select(l => (l.ItemId!.Value, l.Quantity))
                .ToList();
            if (stockLines.Count > 0)
            {
                var received = await _stock.ReceiveStockAsync(
                    tenantId, entity.WarehouseId.Value, stockLines, voidedBy.ToString());
                if (!received) return false;
            }
        }

        if (entity.PostedJournalId.HasValue && _accounting != null)
        {
            var reversed = await _accounting.ReverseJournalAsync(
                entity.PostedJournalId.Value, tenantId, voidedBy,
                dto.Reason ?? $"Void debit note {entity.DebitNoteNumber}");
            if (reversed == Guid.Empty) return false;
        }

        if (entity.AppliedFinanceBillId.HasValue && entity.TotalAmount > 0)
        {
            var restored = await _bills.UnapplyCreditFromBillAsync(
                entity.AppliedFinanceBillId.Value, entity.TotalAmount, tenantId);
            if (!restored) return false;
        }

        entity.Status = "Voided";
        entity.AppliedFinanceBillId = null;
        entity.PostedJournalId = null;
        entity.StockIssued = false;
        entity.UpdatedBy = voidedBy.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Reason))
        {
            entity.Reason = dto.Reason;
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? $"Voided: {dto.Reason}"
                : $"{entity.Notes}\nVoided: {dto.Reason}";
        }

        await _repository.UpdateAsync(entity);
        return true;
    }

    private async Task RollbackApAndGlAsync(
        Guid billId, decimal amount, Guid? journalId, Guid tenantId, Guid userId)
    {
        if (journalId.HasValue && journalId != Guid.Empty && _accounting != null)
            await _accounting.ReverseJournalAsync(journalId.Value, tenantId, userId, "DN stock issue failed");
        await _bills.UnapplyCreditFromBillAsync(billId, amount, tenantId);
    }

    /// <summary>
    /// Returns journal id on success; <see cref="Guid.Empty"/> if defaults missing (skip GL);
    /// null if posting failed after create (caller should roll back AP).
    /// </summary>
    private async Task<Guid?> TryPostGlJournalAsync(PurchaseDebitNote entity, Guid tenantId, Guid userId)
    {
        if (_accounting == null || _db == null) return Guid.Empty;

        var returnsId = await ResolvePurposeAccountAsync(AccountPurposes.PurchaseReturns, tenantId);
        var apId = await ResolvePurposeAccountAsync(AccountPurposes.AccountsPayable, tenantId);
        if (returnsId == Guid.Empty || apId == Guid.Empty || returnsId == apId)
            return Guid.Empty;

        var inputTaxId = await ResolvePurposeAccountAsync(AccountPurposes.InputTax, tenantId);
        var taxAmount = entity.TaxAmount;
        var returnsCredit = entity.SubTotal > 0 ? entity.SubTotal : entity.TotalAmount - Math.Max(0, taxAmount);
        if (inputTaxId == Guid.Empty || taxAmount <= 0)
        {
            returnsCredit = entity.TotalAmount;
            taxAmount = 0;
        }

        var journalId = await _accounting.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Adjustment",
            JournalDate = entity.DebitNoteDate == default ? DateTime.UtcNow.Date : entity.DebitNoteDate,
            Description = $"Debit note {entity.DebitNoteNumber}",
            ReferenceNo = entity.DebitNoteNumber,
            ReferenceDate = entity.DebitNoteDate
        }, tenantId, userId);
        if (journalId == Guid.Empty) return null;

        var drAp = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = apId,
            Debit = entity.TotalAmount,
            Credit = 0,
            Narration = $"AP debit — {entity.DebitNoteNumber}",
            EntityReferenceTable = "PurchaseDebitNotes",
            EntityReferenceId = entity.Id
        }, journalId, tenantId, userId);

        var crReturns = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = returnsId,
            Debit = 0,
            Credit = returnsCredit,
            Narration = $"Purchase returns — {entity.DebitNoteNumber}",
            EntityReferenceTable = "PurchaseDebitNotes",
            EntityReferenceId = entity.Id
        }, journalId, tenantId, userId);

        Guid crTax = Guid.NewGuid();
        if (taxAmount > 0 && inputTaxId != Guid.Empty)
        {
            crTax = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
            {
                AccountId = inputTaxId,
                Debit = 0,
                Credit = taxAmount,
                Narration = $"Input tax reverse — {entity.DebitNoteNumber}",
                EntityReferenceTable = "PurchaseDebitNotes",
                EntityReferenceId = entity.Id
            }, journalId, tenantId, userId);
        }

        if (drAp == Guid.Empty || crReturns == Guid.Empty || (taxAmount > 0 && crTax == Guid.Empty))
            return null;

        var posted = await _accounting.PostJournalAsync(journalId, tenantId, userId, allowPostWithoutApprove: true);
        return posted == journalId ? journalId : null;
    }

    private async Task<Guid> ResolvePurposeAccountAsync(string purpose, Guid tenantId)
    {
        if (_db == null) return Guid.Empty;
        if (tenantId == Guid.Empty) return Guid.Empty;
        var mapped = await _db.AccountDefaults.AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.Purpose == purpose
                && d.IsActive
                && d.TenantId == tenantId);
        return mapped?.AccountId ?? Guid.Empty;
    }

    private async Task<BillDto?> ResolveApBillAsync(PurchaseInvoice purchaseInvoice, Guid tenantId)
    {
        var apNumber = string.IsNullOrWhiteSpace(purchaseInvoice.InvoiceNumber)
            ? $"AP-{purchaseInvoice.Id:N}"[..16]
            : $"AP-{purchaseInvoice.InvoiceNumber}";

        var bills = await _bills.GetByVendorAsync(purchaseInvoice.VendorId, tenantId);
        return bills.FirstOrDefault(b =>
            string.Equals(b.BillNumber, apNumber, StringComparison.OrdinalIgnoreCase)
            || (b.Notes?.Contains($"PurchaseInvoice:{purchaseInvoice.Id}", StringComparison.OrdinalIgnoreCase) ?? false));
    }

    private async Task SaveLinesAsync(
        Guid debitNoteId,
        Guid tenantId,
        Guid userId,
        IEnumerable<CreatePurchaseDebitNoteLineDto> lines)
    {
        var index = 1;
        foreach (var lineDto in lines)
        {
            var computed = ComputeLineAmounts(lineDto.Quantity, lineDto.UnitPrice, lineDto.TaxRate);
            var line = new PurchaseDebitNoteLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                DebitNoteId = debitNoteId,
                OriginalPurchaseInvoiceLineId = lineDto.OriginalPurchaseInvoiceLineId,
                ItemId = lineDto.ItemId,
                Description = lineDto.Description ?? string.Empty,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                TaxRate = lineDto.TaxRate,
                LineTotal = computed.LineTotal,
                LineNumber = lineDto.LineNumber > 0 ? lineDto.LineNumber : index,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _lineRepository.CreateAsync(line);
            index++;
        }
    }

    private static void ApplyLineTotals(PurchaseDebitNote entity, IEnumerable<CreatePurchaseDebitNoteLineDto> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0) return;

        decimal subTotal = 0, taxAmount = 0;
        foreach (var line in lineList)
        {
            var computed = ComputeLineAmounts(line.Quantity, line.UnitPrice, line.TaxRate);
            subTotal += computed.NetBeforeTax;
            taxAmount += computed.TaxAmount;
        }

        entity.SubTotal = Math.Round(subTotal, 2);
        entity.TaxAmount = Math.Round(taxAmount, 2);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;
    }

    private static (decimal NetBeforeTax, decimal TaxAmount, decimal LineTotal) ComputeLineAmounts(
        decimal quantity, decimal unitPrice, decimal taxRate)
    {
        var net = Math.Round(quantity * unitPrice, 2);
        var tax = Math.Round(net * taxRate / 100m, 2);
        return (net, tax, net + tax);
    }

    private static bool IsImmutable(string status) =>
        string.Equals(status, "Posted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Voided", StringComparison.OrdinalIgnoreCase);

    private static CreatePurchaseDebitNoteLineDto ToCreateLine(UpdatePurchaseDebitNoteLineDto line) => new()
    {
        OriginalPurchaseInvoiceLineId = line.OriginalPurchaseInvoiceLineId,
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        TaxRate = line.TaxRate,
        LineNumber = line.LineNumber
    };

    private static PurchaseDebitNoteDto MapToDto(PurchaseDebitNote entity) => new()
    {
        Id = entity.Id,
        DebitNoteNumber = entity.DebitNoteNumber,
        DebitNoteDate = entity.DebitNoteDate,
        VendorId = entity.VendorId,
        OriginalPurchaseInvoiceId = entity.OriginalPurchaseInvoiceId,
        PurchaseReturnId = entity.PurchaseReturnId,
        WarehouseId = entity.WarehouseId,
        StockIssued = entity.StockIssued,
        Status = entity.Status,
        Reason = entity.Reason,
        SubTotal = entity.SubTotal,
        TaxAmount = entity.TaxAmount,
        TotalAmount = entity.TotalAmount,
        Notes = entity.Notes,
        PostedAt = entity.PostedAt,
        PostedBy = entity.PostedBy,
        AppliedFinanceBillId = entity.AppliedFinanceBillId,
        PostedJournalId = entity.PostedJournalId,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static PurchaseDebitNoteLineDto MapLineToDto(PurchaseDebitNoteLine line) => new()
    {
        Id = line.Id,
        DebitNoteId = line.DebitNoteId,
        LineNumber = line.LineNumber,
        OriginalPurchaseInvoiceLineId = line.OriginalPurchaseInvoiceLineId,
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        TaxRate = line.TaxRate,
        LineTotal = line.LineTotal
    };

    private static PurchaseDebitNote MapToEntity(CreatePurchaseDebitNoteDto dto, Guid tenantId, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        DebitNoteNumber = dto.DebitNoteNumber,
        DebitNoteDate = dto.DebitNoteDate,
        VendorId = dto.VendorId,
        OriginalPurchaseInvoiceId = dto.OriginalPurchaseInvoiceId,
        PurchaseReturnId = dto.PurchaseReturnId,
        WarehouseId = dto.WarehouseId,
        Status = "Draft",
        Reason = dto.Reason,
        Notes = dto.Notes,
        CreatedBy = userId.ToString(),
        CreatedAt = DateTime.UtcNow
    };

    private static void MapToEntity(UpdatePurchaseDebitNoteDto dto, PurchaseDebitNote entity, Guid userId)
    {
        entity.DebitNoteNumber = dto.DebitNoteNumber;
        entity.DebitNoteDate = dto.DebitNoteDate;
        entity.VendorId = dto.VendorId;
        entity.OriginalPurchaseInvoiceId = dto.OriginalPurchaseInvoiceId;
        entity.PurchaseReturnId = dto.PurchaseReturnId;
        entity.WarehouseId = dto.WarehouseId;
        entity.Reason = dto.Reason;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
