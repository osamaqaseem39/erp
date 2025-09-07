using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class BillMapper
{
    public static BillDto ToDto(Bill entity)
    {
        return new BillDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            BillNumber = entity.BillNumber,
            VendorId = entity.VendorId,
            VendorName = string.Empty, // Will be populated by service layer
            BillDate = entity.BillDate,
            DueDate = entity.DueDate,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            Status = entity.Status,
            Notes = entity.Notes,
            ApprovedDate = entity.ApprovedDate,
            PaidDate = entity.PaidDate,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Bill ToEntity(CreateBillDto dto, Guid tenantId, string createdBy)
    {
        return new Bill
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            VendorId = dto.VendorId,
            BillId = Guid.NewGuid(),
            SupplierId = dto.VendorId, // Assuming VendorId and SupplierId are the same
            BillNumber = dto.BillNumber,
            BillDate = dto.BillDate,
            DueDate = dto.DueDate,
            CurrencyCode = "USD", // Default currency
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TaxTotal = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            Balance = dto.TotalAmount,
            Status = dto.Status,
            Notes = dto.Notes,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateBillDto dto, Bill entity, string updatedBy)
    {
        if (dto.BillNumber != null) entity.BillNumber = dto.BillNumber;
        if (dto.VendorId.HasValue) entity.VendorId = dto.VendorId.Value;
        if (dto.VendorId.HasValue) entity.SupplierId = dto.VendorId.Value;
        if (dto.BillDate.HasValue) entity.BillDate = dto.BillDate.Value;
        if (dto.DueDate.HasValue) entity.DueDate = dto.DueDate.Value;
        if (dto.SubTotal.HasValue) entity.SubTotal = dto.SubTotal.Value;
        if (dto.TaxAmount.HasValue) entity.TaxAmount = dto.TaxAmount.Value;
        if (dto.TaxAmount.HasValue) entity.TaxTotal = dto.TaxAmount.Value;
        if (dto.TotalAmount.HasValue) entity.TotalAmount = dto.TotalAmount.Value;
        if (dto.TotalAmount.HasValue) entity.Balance = dto.TotalAmount.Value;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}