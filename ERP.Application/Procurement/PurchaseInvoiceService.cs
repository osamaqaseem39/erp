using ERP.Application.Common;
using ERP.Application.Core.Settings;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Procurement;
using ERP.Shared.Exceptions;

namespace ERP.Application.Procurement;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Approved", "Cancelled" },
            ["Pending"] = new[] { "Approved", "Cancelled" },
            ["Approved"] = new[] { "Paid", "Cancelled" },
        };

    private readonly IPurchaseInvoiceRepository _repository;
    private readonly IPurchaseInvoiceDetailRepository _detailRepository;
    private readonly IBillService _billService;
    private readonly IPurchaseOrderDetailRepository _poDetailRepository;
    private readonly IGoodsReceiptNoteRepository _grnRepository;
    private readonly ICoreSettingsService _settings;

    public PurchaseInvoiceService(
        IPurchaseInvoiceRepository repository,
        IPurchaseInvoiceDetailRepository detailRepository,
        IBillService billService,
        IPurchaseOrderDetailRepository poDetailRepository,
        IGoodsReceiptNoteRepository grnRepository,
        ICoreSettingsService settings)
    {
        _repository = repository;
        _detailRepository = detailRepository;
        _billService = billService;
        _poDetailRepository = poDetailRepository;
        _grnRepository = grnRepository;
        _settings = settings;
    }

    public async Task<PurchaseInvoiceDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;
        var details = (await _detailRepository.GetByInvoiceIdAsync(id, tenantId)).ToList();
        entity.Details = details;
        return MapToDto(entity, true);
    }

    public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(e => MapToDto(e, false));

    public async Task<Guid> CreateAsync(CreatePurchaseInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var entity = new PurchaseInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            POId = dto.POId ?? dto.PurchaseOrderId,
            VendorId = dto.VendorId,
            InvoiceNumber = string.IsNullOrWhiteSpace(dto.InvoiceNumber)
                ? $"PI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
                : dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(30),
            Currency = dto.Currency ?? "USD",
            Status = "Pending",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        ApplyLineTotals(entity, dto.Details);
        var id = await _repository.CreateAsync(entity);
        await SaveDetailsAsync(id, tenantId, userId, dto.Details);
        return id;
    }

    public async Task<bool> UpdateAsync(UpdatePurchaseInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null || IsImmutable(entity.Status)) return false;

        if (dto.CompanyId.HasValue) entity.CompanyId = dto.CompanyId.Value;
        if (dto.VendorId.HasValue) entity.VendorId = dto.VendorId.Value;
        var poId = dto.POId ?? dto.PurchaseOrderId;
        if (poId.HasValue) entity.POId = poId;
        if (!string.IsNullOrWhiteSpace(dto.InvoiceNumber)) entity.InvoiceNumber = dto.InvoiceNumber;
        if (dto.InvoiceDate.HasValue) entity.InvoiceDate = dto.InvoiceDate.Value;
        if (dto.DueDate.HasValue) entity.DueDate = dto.DueDate.Value;
        if (!string.IsNullOrWhiteSpace(dto.Currency)) entity.Currency = dto.Currency;
        // ERP-FIX-5: status only via approve/reject
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        ApplyLineTotals(entity, dto.Details);
        await _detailRepository.DeleteByInvoiceIdAsync(entity.Id, tenantId);
        await SaveDetailsAsync(entity.Id, tenantId, userId, dto.Details);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null || IsImmutable(entity.Status)) return false;
        var status = DocumentWorkflow.Normalize(entity.Status);
        if (!string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
            return false;
        await _detailRepository.DeleteByInvoiceIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<PurchaseInvoiceDto>> GetByVendorAsync(Guid vendorId, Guid tenantId) =>
        (await _repository.GetByVendorAsync(vendorId, tenantId)).Select(e => MapToDto(e, false));

    public async Task<IEnumerable<PurchaseInvoiceDto>> GetByStatusAsync(string status, Guid tenantId) =>
        (await _repository.GetByStatusAsync(status, tenantId)).Select(e => MapToDto(e, false));

    public async Task<IEnumerable<PurchaseInvoiceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId) =>
        (await _repository.GetByDateRangeAsync(startDate, endDate, tenantId)).Select(e => MapToDto(e, false));

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Approved", Transitions))
            return false;
        // ERP-FIX-6: creator cannot final-approve
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId))
            return false;

        await EnforceThreeWayMatchAsync(entity, tenantId);

        var subTotal = Math.Round(entity.TotalAmount - entity.TaxAmount, 2);
        await _billService.CreateAsync(new CreateBillDto
        {
            CompanyId = entity.CompanyId,
            BillNumber = $"AP-{entity.InvoiceNumber}",
            VendorId = entity.VendorId,
            BillDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            SubTotal = subTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            Status = "Issued",
            CurrencyCode = entity.Currency,
            PurchaseOrderId = entity.POId,
            PurchaseInvoiceId = entity.Id,
            Reference = entity.InvoiceNumber,
            Notes = $"AP bill from purchase invoice {entity.InvoiceNumber}"
        }, tenantId, userId);

        entity.Status = "Approved";
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

        // Reverse AP bill created on Approve when still unpaid
        if (string.Equals(entity.Status, "Approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            var tag = $"PurchaseInvoice:{entity.Id}";
            var apNumber = $"AP-{entity.InvoiceNumber}";
            var bills = await _billService.GetByVendorAsync(entity.VendorId, tenantId);
            var ap = bills.FirstOrDefault(b =>
                b.SourcePurchaseInvoiceId == entity.Id
                || string.Equals(b.BillNumber, apNumber, StringComparison.OrdinalIgnoreCase)
                || (b.Notes?.Contains(tag, StringComparison.OrdinalIgnoreCase) ?? false));
            if (ap != null
                && !string.Equals(ap.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(ap.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _billService.VoidBillAsync(
                        ap.Id,
                        new VoidBillDto
                        {
                            BillId = ap.Id,
                            Reason = "Purchase invoice cancelled",
                            Notes = tag
                        },
                        tenantId,
                        userId);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task SaveDetailsAsync(Guid invoiceId, Guid tenantId, Guid userId, IEnumerable<CreatePurchaseInvoiceDetailDto> details)
    {
        foreach (var line in details)
        {
            var taxRate = line.TaxRate ?? 0;
            var netBeforeTax = Math.Round(line.Quantity * line.UnitPrice, 2);
            var tax = Math.Round(netBeforeTax * taxRate / 100m, 2);
            await _detailRepository.CreateAsync(new PurchaseInvoiceDetail
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TaxAmount = tax,
                NetAmount = netBeforeTax + tax,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static void ApplyLineTotals(PurchaseInvoice entity, IEnumerable<CreatePurchaseInvoiceDetailDto> details)
    {
        var list = details.ToList();
        if (list.Count == 0) return;

        decimal tax = 0, net = 0;
        foreach (var line in list)
        {
            var taxRate = line.TaxRate ?? 0;
            var netBeforeTax = Math.Round(line.Quantity * line.UnitPrice, 2);
            var lineTax = Math.Round(netBeforeTax * taxRate / 100m, 2);
            tax += lineTax;
            net += netBeforeTax + lineTax;
        }

        entity.TaxAmount = Math.Round(tax, 2);
        entity.TotalAmount = Math.Round(net, 2);
        entity.NetPayable = entity.TotalAmount;
    }

    private async Task EnforceThreeWayMatchAsync(PurchaseInvoice entity, Guid tenantId)
    {
        var enabledRaw = await _settings.GetValueAsync(SettingsCatalog.ThreeWayMatchKey);
        if (!bool.TryParse(enabledRaw, out var enabled) || !enabled)
            return;
        if (!entity.POId.HasValue || entity.POId == Guid.Empty)
            throw new ConflictException("Three-way match requires a linked purchase order on the invoice.");

        var toleranceRaw = await _settings.GetValueAsync(SettingsCatalog.ThreeWayMatchToleranceKey);
        if (!decimal.TryParse(toleranceRaw, out var tolerancePercent))
            tolerancePercent = 2m;

        var piLines = (await _detailRepository.GetByInvoiceIdAsync(entity.Id, tenantId)).ToList();
        var poLines = (await _poDetailRepository.GetByPurchaseOrderIdAsync(entity.POId.Value, tenantId)).ToList();
        var grns = (await _grnRepository.GetByPurchaseOrderAsync(entity.POId.Value, tenantId)).ToList();
        var grnQtyByItem = grns
            .SelectMany(g => g.Details)
            .GroupBy(d => d.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantityReceived));

        var variances = new List<ThreeWayMatchVarianceDto>();
        foreach (var line in piLines)
        {
            var po = poLines.FirstOrDefault(p => p.ItemId == line.ItemId);
            if (po == null)
            {
                variances.Add(new ThreeWayMatchVarianceDto
                {
                    ItemId = line.ItemId,
                    InvoiceQty = line.Quantity,
                    InvoicePrice = line.UnitPrice,
                    Message = "No matching PO line for invoice item."
                });
                continue;
            }

            grnQtyByItem.TryGetValue(line.ItemId, out var grnQty);
            var qtyOk = WithinTolerance(line.Quantity, po.QuantityOrdered, tolerancePercent)
                        && WithinTolerance(line.Quantity, grnQty, tolerancePercent);
            var priceOk = WithinTolerance(line.UnitPrice, po.UnitPrice, tolerancePercent);
            if (qtyOk && priceOk) continue;

            variances.Add(new ThreeWayMatchVarianceDto
            {
                ItemId = line.ItemId,
                InvoiceQty = line.Quantity,
                PoQty = po.QuantityOrdered,
                GrnQty = grnQty,
                InvoicePrice = line.UnitPrice,
                PoPrice = po.UnitPrice,
                Message = $"Qty/price variance exceeds {tolerancePercent}% tolerance."
            });
        }

        if (variances.Count > 0)
        {
            var summary = string.Join("; ", variances.Select(v =>
                $"Item {v.ItemId}: PI qty {v.InvoiceQty} @ {v.InvoicePrice}, PO qty {v.PoQty} @ {v.PoPrice}, GRN qty {v.GrnQty}"));
            throw new ConflictException($"Three-way match failed: {summary}");
        }
    }

    private static bool WithinTolerance(decimal actual, decimal expected, decimal tolerancePercent)
    {
        if (expected == 0) return actual == 0;
        var diff = Math.Abs(actual - expected);
        var pct = diff / Math.Abs(expected) * 100m;
        return pct <= tolerancePercent;
    }

    private static bool IsImmutable(string status) =>
        string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase);

    private static PurchaseInvoiceDto MapToDto(PurchaseInvoice entity, bool includeDetails)
    {
        var details = includeDetails && entity.Details != null
            ? entity.Details.Select(d => new PurchaseInvoiceDetailDto
            {
                Id = d.Id,
                TenantId = d.TenantId,
                InvoiceId = d.InvoiceId,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TaxAmount = d.TaxAmount,
                NetAmount = d.NetAmount,
                TotalPrice = d.NetAmount,
                CreatedAt = d.CreatedAt,
                CreatedBy = d.CreatedBy
            }).ToList()
            : null;

        var subTotal = entity.Details?.Sum(d => d.NetAmount - d.TaxAmount) ?? (entity.TotalAmount - entity.TaxAmount);

        return new PurchaseInvoiceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            InvoiceNumber = entity.InvoiceNumber,
            VendorId = entity.VendorId,
            PurchaseOrderId = entity.POId,
            POId = entity.POId,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            Currency = entity.Currency,
            SubTotal = Math.Round(subTotal, 2),
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            NetPayable = entity.NetPayable,
            BalanceAmount = entity.NetPayable,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive,
            Details = details,
            InvoiceDetails = details
        };
    }
}
