using ERP.Application.Common;
using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Procurement;

public class PurchaseOrderService : IPurchaseOrderService
{
    internal static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "PendingApproval", "Issued", "Cancelled" },
            ["PendingApproval"] = new[] { "Issued", "Cancelled" },
            ["Issued"] = new[] { "PartiallyReceived", "Completed", "Cancelled" },
            ["PartiallyReceived"] = new[] { "Completed", "Cancelled" },
        };

    /// <summary>Subset used when GRN posting advances PO receipt status.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> ReceiptTransitions => Transitions;

    private readonly IPurchaseOrderRepository _repository;
    private readonly IPurchaseOrderDetailRepository _detailRepository;

    public PurchaseOrderService(
        IPurchaseOrderRepository repository,
        IPurchaseOrderDetailRepository detailRepository)
    {
        _repository = repository;
        _detailRepository = detailRepository;
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapToDto(entity, includeDetails: true);
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(e => MapToDto(e, includeDetails: false));
    }

    public async Task<Guid> CreateAsync(CreatePurchaseOrderDto dto, Guid tenantId, Guid userId)
    {
        var entity = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            RFQId = dto.RFQId,
            VendorId = dto.VendorId,
            PurchaseOrderNumber = string.IsNullOrWhiteSpace(dto.PurchaseOrderNumber)
                ? $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
                : dto.PurchaseOrderNumber,
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate ?? DateTime.UtcNow.AddDays(14),
            Currency = dto.Currency ?? "USD",
            PaymentTerms = dto.PaymentTerms ?? string.Empty,
            DeliveryTerms = dto.DeliveryTerms ?? dto.Terms ?? string.Empty,
            Status = "Draft",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        ApplyLineTotals(entity, dto.Details);
        var id = await _repository.CreateAsync(entity);
        await SaveDetailsAsync(id, tenantId, userId, dto.Details);
        return id;
    }

    public async Task<bool> UpdateAsync(UpdatePurchaseOrderDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (IsImmutable(entity.Status)) return false;

        if (dto.CompanyId.HasValue) entity.CompanyId = dto.CompanyId.Value;
        if (dto.VendorId.HasValue) entity.VendorId = dto.VendorId.Value;
        if (dto.RFQId.HasValue) entity.RFQId = dto.RFQId;
        if (!string.IsNullOrWhiteSpace(dto.PurchaseOrderNumber)) entity.PurchaseOrderNumber = dto.PurchaseOrderNumber;
        if (dto.OrderDate.HasValue) entity.OrderDate = dto.OrderDate.Value;
        if (dto.ExpectedDeliveryDate.HasValue) entity.ExpectedDeliveryDate = dto.ExpectedDeliveryDate.Value;
        if (!string.IsNullOrWhiteSpace(dto.Currency)) entity.Currency = dto.Currency;
        if (dto.PaymentTerms != null) entity.PaymentTerms = dto.PaymentTerms;
        if (dto.DeliveryTerms != null) entity.DeliveryTerms = dto.DeliveryTerms;
        else if (dto.Terms != null) entity.DeliveryTerms = dto.Terms;
        // ERP-FIX-5: status only via approve/reject (or GRN receipt side-effects)
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        ApplyLineTotals(entity, dto.Details);
        await _detailRepository.DeleteByPurchaseOrderIdAsync(entity.Id, tenantId);
        await SaveDetailsAsync(entity.Id, tenantId, userId, dto.Details);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (IsImmutable(entity.Status)) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        await _detailRepository.DeleteByPurchaseOrderIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetByVendorAsync(Guid vendorId, Guid tenantId)
    {
        var entities = await _repository.GetByVendorAsync(vendorId, tenantId);
        return entities.Select(e => MapToDto(e, includeDetails: false));
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(e => MapToDto(e, includeDetails: false));
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
        return entities.Select(e => MapToDto(e, includeDetails: false));
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Issued", Transitions))
            return false;
        // ERP-FIX-6: creator cannot final-approve
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId))
            return false;

        entity.Status = "Issued";
        entity.ApprovedBy = userId.ToString();
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

    private async Task SaveDetailsAsync(
        Guid poId,
        Guid tenantId,
        Guid userId,
        IEnumerable<CreatePurchaseOrderDetailDto> details)
    {
        foreach (var lineDto in details)
        {
            var computed = ComputeLineAmounts(lineDto.Quantity, lineDto.UnitPrice, lineDto.Discount, lineDto.TaxRate);
            var detail = new PurchaseOrderDetail
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                POId = poId,
                ItemId = lineDto.ItemId,
                QuantityOrdered = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                Discount = lineDto.Discount,
                TaxAmount = computed.TaxAmount,
                NetAmount = computed.NetAmount,
                ReceivedQuantity = 0,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _detailRepository.CreateAsync(detail);
        }
    }

    private static void ApplyLineTotals(PurchaseOrder entity, IEnumerable<CreatePurchaseOrderDetailDto> details)
    {
        var list = details.ToList();
        if (list.Count == 0) return;

        decimal subTotal = 0, taxAmount = 0;
        foreach (var line in list)
        {
            var computed = ComputeLineAmounts(line.Quantity, line.UnitPrice, line.Discount, line.TaxRate);
            subTotal += computed.NetBeforeTax;
            taxAmount += computed.TaxAmount;
        }

        entity.TotalAmount = Math.Round(subTotal + taxAmount, 2);
    }

    private static (decimal NetBeforeTax, decimal TaxAmount, decimal NetAmount) ComputeLineAmounts(
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxRate)
    {
        var gross = quantity * unitPrice;
        var netBeforeTax = Math.Round(gross * (1 - discountPercent / 100m), 2);
        var tax = Math.Round(netBeforeTax * taxRate / 100m, 2);
        return (netBeforeTax, tax, netBeforeTax + tax);
    }

    private static bool IsImmutable(string status) =>
        string.Equals(status, "Issued", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "PartiallyReceived", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    private static PurchaseOrderDto MapToDto(PurchaseOrder entity, bool includeDetails)
    {
        var details = includeDetails && entity.Details != null
            ? entity.Details.Select(MapDetailToDto).ToList()
            : null;

        decimal subTotal = 0, taxAmount = 0;
        if (entity.Details != null)
        {
            foreach (var d in entity.Details)
            {
                var lineNet = d.NetAmount - d.TaxAmount;
                subTotal += lineNet;
                taxAmount += d.TaxAmount;
            }
        }

        return new PurchaseOrderDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            PurchaseOrderNumber = entity.PurchaseOrderNumber,
            VendorId = entity.VendorId,
            VendorName = entity.Vendor?.VendorName,
            VendorCode = entity.Vendor?.VendorCode,
            RFQId = entity.RFQId,
            OrderDate = entity.OrderDate,
            ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
            Currency = entity.Currency,
            PaymentTerms = entity.PaymentTerms,
            DeliveryTerms = entity.DeliveryTerms,
            SubTotal = Math.Round(subTotal, 2),
            TaxAmount = Math.Round(taxAmount, 2),
            TotalAmount = entity.TotalAmount,
            Status = entity.Status,
            ApprovedBy = string.IsNullOrEmpty(entity.ApprovedBy) ? null : entity.ApprovedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive,
            Details = details,
            PurchaseOrderDetails = details
        };
    }

    private static PurchaseOrderDetailDto MapDetailToDto(PurchaseOrderDetail detail) => new()
    {
        Id = detail.Id,
        TenantId = detail.TenantId,
        POId = detail.POId,
        PurchaseOrderId = detail.POId,
        ItemId = detail.ItemId,
        Quantity = detail.QuantityOrdered,
        QuantityOrdered = detail.QuantityOrdered,
        UnitPrice = detail.UnitPrice,
        Discount = detail.Discount,
        TaxAmount = detail.TaxAmount,
        NetAmount = detail.NetAmount,
        TotalPrice = detail.NetAmount,
        ReceivedQuantity = detail.ReceivedQuantity,
        CreatedAt = detail.CreatedAt,
        CreatedBy = detail.CreatedBy
    };
}
