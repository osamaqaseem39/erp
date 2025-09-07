using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Shared.Mappers.CRM;

public static class OpportunityMapper
{
    public static OpportunityDto ToDto(this Opportunity entity)
    {
        return new OpportunityDto
        {
            Id = entity.Id,
            OpportunityCode = entity.OpportunityCode,
            OpportunityName = entity.OpportunityName,
            CustomerId = entity.CustomerId,
            LeadId = entity.LeadId,
            Stage = entity.Stage,
            EstimatedValue = entity.EstimatedValue,
            ActualValue = entity.ActualValue,
            Probability = entity.Probability,
            ExpectedCloseDate = entity.ExpectedCloseDate,
            ActualCloseDate = entity.ActualCloseDate,
            Description = entity.Description,
            AssignedToUserId = entity.AssignedToUserId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CustomerName = entity.Customer?.CustomerName,
            LeadName = entity.Lead?.FirstName + " " + entity.Lead?.LastName,
            AssignedToUserName = entity.AssignedToUser?.Username
        };
    }

    public static Opportunity ToEntity(this OpportunityDto dto)
    {
        return new Opportunity
        {
            Id = dto.Id,
            OpportunityCode = dto.OpportunityCode,
            OpportunityName = dto.OpportunityName,
            CustomerId = dto.CustomerId,
            LeadId = dto.LeadId,
            Stage = dto.Stage,
            EstimatedValue = dto.EstimatedValue ?? 0m,
            ActualValue = dto.ActualValue,
            Probability = dto.Probability ?? 0m,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            ActualCloseDate = dto.ActualCloseDate,
            Description = dto.Description,
            AssignedToUserId = dto.AssignedToUserId,
            IsActive = dto.IsActive ?? false,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}