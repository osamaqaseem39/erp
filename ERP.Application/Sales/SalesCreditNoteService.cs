using ERP.Application.Common;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.Sales;

public class SalesCreditNoteService : ISalesCreditNoteService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Posted" },
            ["Posted"] = new[] { "Voided" },
        };

    private readonly ISalesCreditNoteRepository _repository;
    private readonly ISalesCreditNoteLineRepository _lineRepository;
    private readonly ISalesInvoiceRepository _salesInvoiceRepository;
    private readonly IInvoiceService _financeInvoiceService;
    private readonly IAccountingService? _accounting;
    private readonly ERPDbContext? _db;
    private readonly IStockService? _stock;

    public SalesCreditNoteService(
        ISalesCreditNoteRepository repository,
        ISalesCreditNoteLineRepository lineRepository,
        ISalesInvoiceRepository salesInvoiceRepository,
        IInvoiceService financeInvoiceService,
        IAccountingService? accounting = null,
        ERPDbContext? db = null,
        IStockService? stock = null)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _salesInvoiceRepository = salesInvoiceRepository;
        _financeInvoiceService = financeInvoiceService;
        _accounting = accounting;
        _db = db;
        _stock = stock;
    }

    public async Task<SalesCreditNoteDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;

        var dto = MapToDto(entity);
        var lines = await _lineRepository.GetByCreditNoteIdAsync(id, tenantId);
        dto.Lines = lines.Select(MapLineToDto).ToList();
        return dto;
    }

    public async Task<IEnumerable<SalesCreditNoteDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesCreditNoteDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesCreditNoteDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateSalesCreditNoteDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        ApplyLineTotals(entity, dto.Lines);

        var created = await _repository.AddAsync(entity);
        await SaveLinesAsync(created.Id, tenantId, userId, dto.Lines);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateSalesCreditNoteDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (IsImmutable(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity, userId);
        // ERP-FIX-5: status only via post/void
        entity.Status = previousStatus;
        ApplyLineTotals(entity, dto.Lines.Select(ToCreateLine));

        await _lineRepository.DeleteByCreditNoteIdAsync(entity.Id, tenantId);
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

        await _lineRepository.DeleteByCreditNoteIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> PostAsync(Guid creditNoteId, Guid tenantId, Guid postedBy)
    {
        var entity = await _repository.GetByIdAsync(creditNoteId, tenantId);
        if (entity == null) return false;

        // Idempotent: already posted
        if (string.Equals(DocumentWorkflow.Normalize(entity.Status), "Posted", StringComparison.OrdinalIgnoreCase)
            && entity.AppliedFinanceInvoiceId.HasValue)
            return true;

        if (!DocumentWorkflow.CanTransition(entity.Status, "Posted", Transitions))
            return false;

        if (entity.TotalAmount <= 0) return false;

        var salesInvoice = await _salesInvoiceRepository.GetByIdAsync(entity.OriginalSalesInvoiceId, tenantId);
        if (salesInvoice == null) return false;
        if (!string.Equals(salesInvoice.Status, "Posted", StringComparison.OrdinalIgnoreCase))
            return false;
        if (salesInvoice.CustomerId != entity.CustomerId)
            return false;

        var ar = await ResolveArInvoiceAsync(salesInvoice, tenantId);
        if (ar == null) return false;

        if (entity.TotalAmount > ar.Balance)
            return false;

        var applied = await _financeInvoiceService.ApplyCreditToInvoiceAsync(ar.InvoiceId, entity.TotalAmount, tenantId);
        if (!applied) return false;

        Guid? journalId = null;
        if (!entity.PostedJournalId.HasValue)
        {
            journalId = await TryPostGlJournalAsync(entity, tenantId, postedBy);
            if (journalId == Guid.Empty)
            {
                // Defaults missing → AR-only post (same as FA dep when CoA unmapped)
                journalId = null;
            }
            else if (journalId == null)
            {
                await _financeInvoiceService.UnapplyCreditFromInvoiceAsync(ar.InvoiceId, entity.TotalAmount, tenantId);
                return false;
            }
        }
        else
        {
            journalId = entity.PostedJournalId;
        }

        var stockReceived = entity.StockReceived;
        if (!stockReceived
            && entity.WarehouseId.HasValue
            && entity.WarehouseId != Guid.Empty
            && _stock != null)
        {
            var lines = await _lineRepository.GetByCreditNoteIdAsync(entity.Id, tenantId);
            var stockLines = lines
                .Where(l => l.ItemId.HasValue && l.ItemId != Guid.Empty && l.Quantity > 0)
                .Select(l => (l.ItemId!.Value, l.Quantity))
                .ToList();
            if (stockLines.Count > 0)
            {
                var received = await _stock.ReceiveStockAsync(
                    tenantId, entity.WarehouseId.Value, stockLines, postedBy.ToString());
                if (!received)
                {
                    await RollbackArAndGlAsync(ar.InvoiceId, entity.TotalAmount, journalId, tenantId, postedBy);
                    return false;
                }
                stockReceived = true;
            }
        }

        entity.Status = "Posted";
        entity.PostedAt = DateTime.UtcNow;
        entity.PostedBy = postedBy;
        entity.AppliedFinanceInvoiceId = ar.InvoiceId;
        entity.PostedJournalId = journalId;
        entity.StockReceived = stockReceived;
        entity.UpdatedBy = postedBy.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> VoidAsync(Guid creditNoteId, VoidSalesCreditNoteDto dto, Guid tenantId, Guid voidedBy)
    {
        var entity = await _repository.GetByIdAsync(creditNoteId, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Voided", Transitions))
            return false;

        if (entity.StockReceived
            && entity.WarehouseId.HasValue
            && entity.WarehouseId != Guid.Empty
            && _stock != null)
        {
            var lines = await _lineRepository.GetByCreditNoteIdAsync(entity.Id, tenantId);
            var stockLines = lines
                .Where(l => l.ItemId.HasValue && l.ItemId != Guid.Empty && l.Quantity > 0)
                .Select(l => (l.ItemId!.Value, l.Quantity))
                .ToList();
            if (stockLines.Count > 0)
            {
                var issued = await _stock.IssueStockAsync(
                    tenantId, entity.WarehouseId.Value, stockLines, voidedBy.ToString());
                if (!issued) return false;
            }
        }

        if (entity.PostedJournalId.HasValue && _accounting != null)
        {
            var reversed = await _accounting.ReverseJournalAsync(
                entity.PostedJournalId.Value, tenantId, voidedBy,
                dto.Reason ?? $"Void credit note {entity.CreditNoteNumber}");
            if (reversed == Guid.Empty) return false;
        }

        if (entity.AppliedFinanceInvoiceId.HasValue && entity.TotalAmount > 0)
        {
            var restored = await _financeInvoiceService.UnapplyCreditFromInvoiceAsync(
                entity.AppliedFinanceInvoiceId.Value, entity.TotalAmount, tenantId);
            if (!restored) return false;
        }

        entity.Status = "Voided";
        entity.AppliedFinanceInvoiceId = null;
        entity.PostedJournalId = null;
        entity.StockReceived = false;
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

    private async Task RollbackArAndGlAsync(
        Guid arInvoiceId, decimal amount, Guid? journalId, Guid tenantId, Guid userId)
    {
        if (journalId.HasValue && journalId != Guid.Empty && _accounting != null)
            await _accounting.ReverseJournalAsync(journalId.Value, tenantId, userId, "CN stock receive failed");
        await _financeInvoiceService.UnapplyCreditFromInvoiceAsync(arInvoiceId, amount, tenantId);
    }

    /// <summary>
    /// Returns journal id on success; <see cref="Guid.Empty"/> if defaults missing (skip GL);
    /// null if posting failed after create (caller should roll back AR).
    /// </summary>
    private async Task<Guid?> TryPostGlJournalAsync(SalesCreditNote entity, Guid tenantId, Guid userId)
    {
        if (_accounting == null || _db == null) return Guid.Empty;

        var returnsId = await ResolvePurposeAccountAsync(AccountPurposes.SalesReturns, tenantId);
        var arId = await ResolvePurposeAccountAsync(AccountPurposes.AccountsReceivable, tenantId);
        if (returnsId == Guid.Empty || arId == Guid.Empty || returnsId == arId)
            return Guid.Empty;

        var outputTaxId = await ResolvePurposeAccountAsync(AccountPurposes.OutputTax, tenantId);
        var taxAmount = entity.TaxAmount;
        var returnsDebit = entity.SubTotal > 0 ? entity.SubTotal : entity.TotalAmount - Math.Max(0, taxAmount);
        if (outputTaxId == Guid.Empty || taxAmount <= 0)
        {
            returnsDebit = entity.TotalAmount;
            taxAmount = 0;
        }

        var journalId = await _accounting.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Adjustment",
            JournalDate = entity.CreditNoteDate == default ? DateTime.UtcNow.Date : entity.CreditNoteDate,
            Description = $"Credit note {entity.CreditNoteNumber}",
            ReferenceNo = entity.CreditNoteNumber,
            ReferenceDate = entity.CreditNoteDate
        }, tenantId, userId);
        if (journalId == Guid.Empty) return null;

        var drReturns = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = returnsId,
            Debit = returnsDebit,
            Credit = 0,
            Narration = $"Sales returns — {entity.CreditNoteNumber}",
            EntityReferenceTable = "SalesCreditNotes",
            EntityReferenceId = entity.Id
        }, journalId, tenantId, userId);

        Guid drTax = Guid.NewGuid();
        if (taxAmount > 0 && outputTaxId != Guid.Empty)
        {
            drTax = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
            {
                AccountId = outputTaxId,
                Debit = taxAmount,
                Credit = 0,
                Narration = $"Output tax reverse — {entity.CreditNoteNumber}",
                EntityReferenceTable = "SalesCreditNotes",
                EntityReferenceId = entity.Id
            }, journalId, tenantId, userId);
        }

        var crAr = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = arId,
            Debit = 0,
            Credit = entity.TotalAmount,
            Narration = $"AR credit — {entity.CreditNoteNumber}",
            EntityReferenceTable = "SalesCreditNotes",
            EntityReferenceId = entity.Id
        }, journalId, tenantId, userId);

        if (drReturns == Guid.Empty || crAr == Guid.Empty || (taxAmount > 0 && drTax == Guid.Empty))
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

    private async Task<InvoiceDto?> ResolveArInvoiceAsync(SalesInvoice salesInvoice, Guid tenantId)
    {
        var arNumber = string.IsNullOrWhiteSpace(salesInvoice.InvoiceNumber)
            ? $"AR-{salesInvoice.Id:N}"[..16]
            : salesInvoice.InvoiceNumber;

        var (existing, _) = await _financeInvoiceService.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = tenantId,
            CustomerId = salesInvoice.CustomerId,
            PageNumber = 1,
            PageSize = 100
        });

        return existing.FirstOrDefault(i =>
            string.Equals(i.InvoiceNumber, arNumber, StringComparison.OrdinalIgnoreCase)
            || (i.Notes?.Contains($"SalesInvoice:{salesInvoice.Id}", StringComparison.OrdinalIgnoreCase) ?? false));
    }

    private async Task SaveLinesAsync(
        Guid creditNoteId,
        Guid tenantId,
        Guid userId,
        IEnumerable<CreateSalesCreditNoteLineDto> lines)
    {
        var index = 1;
        foreach (var lineDto in lines)
        {
            var computed = ComputeLineAmounts(lineDto.Quantity, lineDto.UnitPrice, lineDto.TaxRate);
            var line = new SalesCreditNoteLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreditNoteId = creditNoteId,
                OriginalSalesInvoiceLineId = lineDto.OriginalSalesInvoiceLineId,
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

    private static void ApplyLineTotals(SalesCreditNote entity, IEnumerable<CreateSalesCreditNoteLineDto> lines)
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

    private static CreateSalesCreditNoteLineDto ToCreateLine(UpdateSalesCreditNoteLineDto line) => new()
    {
        OriginalSalesInvoiceLineId = line.OriginalSalesInvoiceLineId,
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        TaxRate = line.TaxRate,
        LineNumber = line.LineNumber
    };

    private static SalesCreditNoteDto MapToDto(SalesCreditNote entity) => new()
    {
        Id = entity.Id,
        CreditNoteNumber = entity.CreditNoteNumber,
        CreditNoteDate = entity.CreditNoteDate,
        CustomerId = entity.CustomerId,
        OriginalSalesInvoiceId = entity.OriginalSalesInvoiceId,
        ReturnOrderId = entity.ReturnOrderId,
        WarehouseId = entity.WarehouseId,
        StockReceived = entity.StockReceived,
        Status = entity.Status,
        Reason = entity.Reason,
        SubTotal = entity.SubTotal,
        TaxAmount = entity.TaxAmount,
        TotalAmount = entity.TotalAmount,
        Notes = entity.Notes,
        PostedAt = entity.PostedAt,
        PostedBy = entity.PostedBy,
        AppliedFinanceInvoiceId = entity.AppliedFinanceInvoiceId,
        PostedJournalId = entity.PostedJournalId,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static SalesCreditNoteLineDto MapLineToDto(SalesCreditNoteLine line) => new()
    {
        Id = line.Id,
        CreditNoteId = line.CreditNoteId,
        LineNumber = line.LineNumber,
        OriginalSalesInvoiceLineId = line.OriginalSalesInvoiceLineId,
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        TaxRate = line.TaxRate,
        LineTotal = line.LineTotal
    };

    private static SalesCreditNote MapToEntity(CreateSalesCreditNoteDto dto, Guid tenantId, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        CreditNoteNumber = dto.CreditNoteNumber,
        CreditNoteDate = dto.CreditNoteDate,
        CustomerId = dto.CustomerId,
        OriginalSalesInvoiceId = dto.OriginalSalesInvoiceId,
        ReturnOrderId = dto.ReturnOrderId,
        WarehouseId = dto.WarehouseId,
        Status = "Draft",
        Reason = dto.Reason,
        Notes = dto.Notes,
        CreatedBy = userId.ToString(),
        CreatedAt = DateTime.UtcNow
    };

    private static void MapToEntity(UpdateSalesCreditNoteDto dto, SalesCreditNote entity, Guid userId)
    {
        entity.CreditNoteNumber = dto.CreditNoteNumber;
        entity.CreditNoteDate = dto.CreditNoteDate;
        entity.CustomerId = dto.CustomerId;
        entity.OriginalSalesInvoiceId = dto.OriginalSalesInvoiceId;
        entity.ReturnOrderId = dto.ReturnOrderId;
        entity.WarehouseId = dto.WarehouseId;
        entity.Reason = dto.Reason;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
