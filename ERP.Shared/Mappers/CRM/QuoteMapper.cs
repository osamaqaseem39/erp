using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Shared.Mappers.CRM;

public static class QuoteMapper
{
    public static QuoteDto ToDto(this Quote entity)
    {
        return new QuoteDto
        {
            Id = entity.Id,
            QuoteNumber = entity.QuoteNumber,
            CustomerId = entity.CustomerId,
            OpportunityId = entity.OpportunityId,
            QuoteDate = entity.QuoteDate,
            ValidUntil = entity.ValidUntil,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            Notes = entity.Notes,
            AssignedToUserId = entity.AssignedToUserId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CustomerName = entity.Customer?.CustomerName,
            OpportunityName = entity.Opportunity?.OpportunityName,
            AssignedToUserName = entity.AssignedToUser?.Username
        };
    }

    public static Quote ToEntity(this QuoteDto dto)
    {
        return new Quote
        {
            Id = dto.Id,
            QuoteNumber = dto.QuoteNumber,
            CustomerId = dto.CustomerId,
            OpportunityId = dto.OpportunityId,
            QuoteDate = dto.QuoteDate ?? DateTime.MinValue,
            ValidUntil = dto.ValidUntil ?? DateTime.MinValue,
            Status = dto.Status,
            SubTotal = dto.SubTotal ?? 0m,
            TaxAmount = dto.TaxAmount ?? 0m,
            TotalAmount = dto.TotalAmount ?? 0m,
            Notes = dto.Notes,
            AssignedToUserId = dto.AssignedToUserId,
            IsActive = dto.IsActive ?? false,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}