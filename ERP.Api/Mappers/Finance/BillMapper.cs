using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class BillMapper : BaseMapper<Bill, BillDto>
{
    public override BillDto Map(Bill source)
    {
        if (source == null) return null!;

        return new BillDto
        {
            Id = source.Id,
            CompanyId = source.CompanyId,
            BillNumber = source.BillNumber,
            VendorId = source.VendorId,
            BillDate = source.BillDate,
            DueDate = source.DueDate,
            SubTotal = source.SubTotal,
            TaxAmount = source.TaxAmount,
            TotalAmount = source.TotalAmount,
            Status = source.Status,
            Notes = source.Notes,
            ApprovedDate = source.ApprovedDate,
            PaidDate = source.PaidDate,
            IsActive = source.IsActive,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Bill Map(BillDto destination)
    {
        if (destination == null) return null!;

        return new Bill
        {
            Id = destination.Id,
            CompanyId = destination.CompanyId,
            BillId = Guid.NewGuid(),
            BillNumber = destination.BillNumber,
            VendorId = destination.VendorId,
            SupplierId = destination.VendorId,
            BillDate = destination.BillDate,
            DueDate = destination.DueDate,
            CurrencyCode = "USD",
            SubTotal = destination.SubTotal,
            TaxAmount = destination.TaxAmount,
            TaxTotal = destination.TaxAmount,
            TotalAmount = destination.TotalAmount,
            Balance = destination.TotalAmount,
            Status = destination.Status,
            Notes = destination.Notes,
            ApprovedDate = destination.ApprovedDate,
            PaidDate = destination.PaidDate,
            IsActive = destination.IsActive,
            TenantId = destination.TenantId,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

