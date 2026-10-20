using ERP.Application.Common;
using ERP.Application.Core.Settings;
using ERP.Application.CRM;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Sales;

public class SalesInvoiceService : ISalesInvoiceService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Posted", "Voided" },
            ["Posted"] = new[] { "Voided" },
        };

    private readonly ISalesInvoiceRepository _repository;
    private readonly ISalesInvoiceLineRepository _lineRepository;
    private readonly IInvoiceService _financeInvoiceService;
    private readonly ICoreSettingsService _settings;
    private readonly ERPDbContext? _db;

    public SalesInvoiceService(
        ISalesInvoiceRepository repository,
        ISalesInvoiceLineRepository lineRepository,
        IInvoiceService financeInvoiceService,
        ICoreSettingsService settings,
        ERPDbContext? db = null)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _financeInvoiceService = financeInvoiceService;
        _settings = settings;
        _db = db;
    }

    public async Task<SalesInvoiceDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;

        var dto = MapToDto(entity);
        await EnrichCustomerNamesAsync(new[] { dto }, tenantId);
        var lines = await _lineRepository.GetByInvoiceIdAsync(id, tenantId);
        dto.Lines = lines.Select(MapLineToDto).ToList();
        return dto;
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        var dtos = entities.Select(MapToDto).ToList();
        await EnrichCustomerNamesAsync(dtos, tenantId);
        return dtos;
    }

    public async Task<Guid> CreateAsync(CreateSalesInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        ApplyLineTotals(entity, dto.Lines);
        entity.BalanceAmount = entity.TotalAmount - entity.PaidAmount;

        var created = await _repository.AddAsync(entity);
        await SaveLinesAsync(created.Id, tenantId, userId, dto.Lines);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateSalesInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (IsImmutable(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity, userId);
        // ERP-FIX-5: status only via post/void actions
        entity.Status = previousStatus;
        ApplyLineTotals(entity, dto.Lines.Select(ToCreateLine));
        entity.BalanceAmount = entity.TotalAmount - entity.PaidAmount;

        await _lineRepository.DeleteByInvoiceIdAsync(entity.Id, tenantId);
        await SaveLinesAsync(entity.Id, tenantId, userId, dto.Lines.Select(ToCreateLine));
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (IsImmutable(entity.Status)) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        await _lineRepository.DeleteByInvoiceIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        var dtos = entities.Select(MapToDto).ToList();
        await EnrichCustomerNamesAsync(dtos, tenantId);
        return dtos;
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        var dtos = entities.Select(MapToDto).ToList();
        await EnrichCustomerNamesAsync(dtos, tenantId);
        return dtos;
    }

    public async Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        var entity = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Posted", Transitions))
            return false;

        var arNumber = string.IsNullOrWhiteSpace(entity.InvoiceNumber)
            ? $"AR-{invoiceId:N}"[..16]
            : entity.InvoiceNumber;

        var (existing, _) = await _financeInvoiceService.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = tenantId,
            CustomerId = entity.CustomerId,
            PageNumber = 1,
            PageSize = 50
        });
        var alreadyLinked = existing.Any(i =>
            i.SourceSalesInvoiceId == entity.Id
            || string.Equals(i.InvoiceNumber, arNumber, StringComparison.OrdinalIgnoreCase)
            || (i.Notes?.Contains($"SalesInvoice:{entity.Id}", StringComparison.OrdinalIgnoreCase) ?? false));

        if (!alreadyLinked)
        {
            await _financeInvoiceService.AddInvoiceAsync(new InvoiceCreateDto
            {
                CustomerId = entity.CustomerId,
                InvoiceNumber = arNumber,
                InvoiceDate = entity.InvoiceDate,
                DueDate = entity.DueDate,
                CurrencyCode = "USD",
                SubTotal = entity.SubTotal,
                TaxAmount = entity.TaxAmount,
                TotalAmount = entity.TotalAmount,
                Status = "Issued",
                SalesOrderId = entity.SalesOrderId,
                SourceSalesInvoiceId = entity.Id,
                Notes = $"SalesInvoice:{entity.Id}"
            }, tenantId, postedBy);
        }

        return await _repository.PostInvoiceAsync(invoiceId, tenantId, postedBy);
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid tenantId, Guid voidedBy)
    {
        var entity = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Voided", Transitions))
            return false;

        // Reverse linked Finance AR (created on Post) when still unpaid
        var arNumber = string.IsNullOrWhiteSpace(entity.InvoiceNumber)
            ? $"AR-{invoiceId:N}"[..16]
            : entity.InvoiceNumber;
        var tag = $"SalesInvoice:{entity.Id}";
        var (existing, _) = await _financeInvoiceService.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = tenantId,
            CustomerId = entity.CustomerId,
            PageNumber = 1,
            PageSize = 50
        });
        var ar = existing.FirstOrDefault(i =>
            i.SourceSalesInvoiceId == entity.Id
            || string.Equals(i.InvoiceNumber, arNumber, StringComparison.OrdinalIgnoreCase)
            || (i.Notes?.Contains(tag, StringComparison.OrdinalIgnoreCase) ?? false));
        if (ar != null
            && !string.Equals(ar.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(ar.Status, "Voided", StringComparison.OrdinalIgnoreCase))
        {
            var arId = ar.InvoiceId != Guid.Empty ? ar.InvoiceId : ar.Id;
            var reversed = await _financeInvoiceService.VoidInvoiceAsync(
                arId,
                new VoidInvoiceDto
                {
                    InvoiceId = arId,
                    Reason = "Sales invoice voided",
                    Notes = tag
                },
                tenantId,
                voidedBy);
            if (!reversed) return false;
        }

        return await _repository.VoidInvoiceAsync(invoiceId, tenantId, voidedBy);
    }

    public async Task<bool> UnpostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid userId)
    {
        var allowRaw = await _settings.GetValueAsync(SettingsCatalog.AllowUnpostKey);
        if (!string.Equals(allowRaw, "true", StringComparison.OrdinalIgnoreCase))
            return false;

        var entity = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Posted", StringComparison.OrdinalIgnoreCase))
            return false;
        if (entity.PaidAmount > 0) return false;

        var arNumber = string.IsNullOrWhiteSpace(entity.InvoiceNumber)
            ? $"AR-{invoiceId:N}"[..16]
            : entity.InvoiceNumber;
        var tag = $"SalesInvoice:{entity.Id}";
        var (existing, _) = await _financeInvoiceService.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = tenantId,
            CustomerId = entity.CustomerId,
            PageNumber = 1,
            PageSize = 50
        });
        var ar = existing.FirstOrDefault(i =>
            i.SourceSalesInvoiceId == entity.Id
            || string.Equals(i.InvoiceNumber, arNumber, StringComparison.OrdinalIgnoreCase)
            || (i.Notes?.Contains(tag, StringComparison.OrdinalIgnoreCase) ?? false));
        if (ar != null
            && !string.Equals(ar.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(ar.Status, "Voided", StringComparison.OrdinalIgnoreCase))
        {
            var arId = ar.InvoiceId != Guid.Empty ? ar.InvoiceId : ar.Id;
            var reversed = await _financeInvoiceService.VoidInvoiceAsync(
                arId,
                new VoidInvoiceDto { InvoiceId = arId, Reason = "Sales invoice unpost", Notes = tag },
                tenantId,
                userId);
            if (!reversed) return false;
        }

        entity.Status = "Draft";
        entity.PostedAt = null;
        entity.PostedBy = null;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    private async Task SaveLinesAsync(
        Guid invoiceId,
        Guid tenantId,
        Guid userId,
        IEnumerable<CreateSalesInvoiceLineDto> lines)
    {
        var index = 1;
        foreach (var lineDto in lines)
        {
            var tax1Rate = lineDto.Tax1Rate ?? lineDto.TaxRate;
            var tax2Rate = lineDto.Tax2Rate ?? 0;
            var computed = ComputeLineAmounts(lineDto.Quantity, lineDto.UnitPrice, discountRate: 0, tax1Rate, tax2Rate);
            var line = new SalesInvoiceLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SalesInvoiceId = invoiceId,
                ItemId = lineDto.ItemId,
                Description = lineDto.Description ?? string.Empty,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                TaxRate = tax1Rate,
                TaxAmount = computed.TaxTotal,
                Tax1Rate = tax1Rate,
                Tax1Amount = computed.Tax1Amount,
                Tax2Rate = tax2Rate,
                Tax2Amount = computed.Tax2Amount,
                LineTotal = computed.LineTotal,
                LineNumber = lineDto.LineNumber > 0 ? lineDto.LineNumber : index,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _lineRepository.CreateAsync(line);
            index++;
        }
    }

    private static void ApplyLineTotals(SalesInvoice entity, IEnumerable<CreateSalesInvoiceLineDto> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0) return;

        decimal subTotal = 0, tax1 = 0, tax2 = 0;
        foreach (var line in lineList)
        {
            var tax1Rate = line.Tax1Rate ?? line.TaxRate;
            var tax2Rate = line.Tax2Rate ?? 0;
            var computed = ComputeLineAmounts(line.Quantity, line.UnitPrice, discountRate: 0, tax1Rate, tax2Rate);
            subTotal += computed.NetBeforeTax;
            tax1 += computed.Tax1Amount;
            tax2 += computed.Tax2Amount;
        }

        entity.SubTotal = Math.Round(subTotal, 2);
        entity.Tax1Amount = Math.Round(tax1, 2);
        entity.Tax2Amount = Math.Round(tax2, 2);
        entity.TaxAmount = entity.Tax1Amount + entity.Tax2Amount;
        entity.TotalAmount = CrmLineCalculator.ApplyDocumentTotal(
            entity.SubTotal + entity.TaxAmount,
            entity.Freight,
            entity.Overhead,
            entity.DiscountType,
            entity.DiscountAmount);
    }

    private static (decimal NetBeforeTax, decimal Tax1Amount, decimal Tax2Amount, decimal TaxTotal, decimal LineTotal)
        ComputeLineAmounts(decimal quantity, decimal unitPrice, decimal discountRate, decimal tax1Rate, decimal tax2Rate)
    {
        var gross = quantity * unitPrice;
        var net = Math.Round(gross * (1 - discountRate / 100m), 2);
        var tax1 = Math.Round(net * tax1Rate / 100m, 2);
        var tax2 = Math.Round(net * tax2Rate / 100m, 2);
        var taxTotal = tax1 + tax2;
        return (net, tax1, tax2, taxTotal, net + taxTotal);
    }

    private static bool IsImmutable(string status) =>
        string.Equals(status, "Posted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Voided", StringComparison.OrdinalIgnoreCase);

    private static CreateSalesInvoiceLineDto ToCreateLine(UpdateSalesInvoiceLineDto line) => new()
    {
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        TaxRate = line.Tax1Rate ?? line.TaxRate,
        Tax1Rate = line.Tax1Rate ?? line.TaxRate,
        Tax2Rate = line.Tax2Rate,
        LineNumber = line.LineNumber
    };

    private async Task EnrichCustomerNamesAsync(IEnumerable<SalesInvoiceDto> dtos, Guid tenantId)
    {
        if (_db == null) return;
        var list = dtos as IList<SalesInvoiceDto> ?? dtos.ToList();
        if (list.Count == 0 || tenantId == Guid.Empty) return;
        var ids = list.Select(d => d.CustomerId).Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0) return;
        var names = await _db.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId && ids.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.CompanyName != null && c.CompanyName != "" ? c.CompanyName : c.CustomerName })
            .ToDictionaryAsync(x => x.Id, x => x.Name);
        foreach (var d in list)
            if (names.TryGetValue(d.CustomerId, out var name))
                d.CustomerName = name;
    }

    private static SalesInvoiceDto MapToDto(SalesInvoice entity) => new()
    {
        Id = entity.Id,
        InvoiceNumber = entity.InvoiceNumber,
        CustomerId = entity.CustomerId,
        InvoiceDate = entity.InvoiceDate,
        DueDate = entity.DueDate,
        Status = entity.Status,
        SubTotal = entity.SubTotal,
        Tax1Amount = entity.Tax1Amount,
        Tax2Amount = entity.Tax2Amount,
        TaxAmount = entity.TaxAmount,
        TotalAmount = entity.TotalAmount,
        PaidAmount = entity.PaidAmount,
        BalanceAmount = entity.BalanceAmount,
        Notes = entity.Notes,
        Terms = entity.Terms,
        SalesOrderId = entity.SalesOrderId,
        DispatchAdviceId = entity.DispatchAdviceId,
        ReferenceNo = entity.ReferenceNo,
        ReferenceDate = entity.ReferenceDate,
        CreditDays = entity.CreditDays,
        PaymentMode = entity.PaymentMode,
        PaymentTermId = entity.PaymentTermId,
        Freight = entity.Freight,
        Overhead = entity.Overhead,
        DiscountType = entity.DiscountType,
        DiscountAmount = entity.DiscountAmount,
        PostedAt = entity.PostedAt,
        PostedBy = entity.PostedBy,
        ProjectId = entity.ProjectId,
        ProjectMilestoneId = entity.ProjectMilestoneId,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static SalesInvoiceLineDto MapLineToDto(SalesInvoiceLine line) => new()
    {
        Id = line.Id,
        SalesInvoiceId = line.SalesInvoiceId,
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        TaxRate = line.Tax1Rate != 0 ? line.Tax1Rate : line.TaxRate,
        TaxAmount = line.Tax1Amount + line.Tax2Amount != 0 ? line.Tax1Amount + line.Tax2Amount : line.TaxAmount,
        Tax1Rate = line.Tax1Rate != 0 ? line.Tax1Rate : line.TaxRate,
        Tax1Amount = line.Tax1Amount != 0 ? line.Tax1Amount : line.TaxAmount,
        Tax2Rate = line.Tax2Rate,
        Tax2Amount = line.Tax2Amount,
        LineTotal = line.LineTotal,
        LineNumber = line.LineNumber
    };

    private static SalesInvoice MapToEntity(CreateSalesInvoiceDto dto, Guid tenantId, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        InvoiceNumber = dto.InvoiceNumber,
        CustomerId = dto.CustomerId,
        InvoiceDate = dto.InvoiceDate,
        DueDate = dto.DueDate,
        Status = "Draft",
        SubTotal = dto.SubTotal,
        TaxAmount = dto.TaxAmount,
        TotalAmount = dto.TotalAmount,
        Notes = dto.Notes,
        Terms = dto.Terms,
        SalesOrderId = dto.SalesOrderId,
        DispatchAdviceId = dto.DispatchAdviceId,
        ReferenceNo = dto.ReferenceNo,
        ReferenceDate = dto.ReferenceDate,
        CreditDays = dto.CreditDays,
        PaymentMode = dto.PaymentMode,
        PaymentTermId = dto.PaymentTermId,
        Freight = dto.Freight,
        Overhead = dto.Overhead,
        DiscountType = dto.DiscountType,
        DiscountAmount = dto.DiscountAmount,
        ProjectId = dto.ProjectId,
        ProjectMilestoneId = dto.ProjectMilestoneId,
        CreatedBy = userId.ToString(),
        CreatedAt = DateTime.UtcNow
    };

    private static void MapToEntity(UpdateSalesInvoiceDto dto, SalesInvoice entity, Guid userId)
    {
        entity.InvoiceNumber = dto.InvoiceNumber;
        entity.CustomerId = dto.CustomerId;
        entity.InvoiceDate = dto.InvoiceDate;
        entity.DueDate = dto.DueDate;
        // ERP-FIX-5: status is NOT free-set from dto; lifecycle endpoints (Post/Void) govern it.
        entity.SubTotal = dto.SubTotal;
        entity.TaxAmount = dto.TaxAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.Notes = dto.Notes;
        entity.Terms = dto.Terms;
        entity.DispatchAdviceId = dto.DispatchAdviceId;
        entity.ReferenceNo = dto.ReferenceNo;
        entity.ReferenceDate = dto.ReferenceDate;
        entity.CreditDays = dto.CreditDays;
        entity.PaymentMode = dto.PaymentMode;
        entity.PaymentTermId = dto.PaymentTermId;
        entity.Freight = dto.Freight;
        entity.Overhead = dto.Overhead;
        entity.DiscountType = dto.DiscountType;
        entity.DiscountAmount = dto.DiscountAmount;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
