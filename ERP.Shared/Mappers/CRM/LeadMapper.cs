using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Shared.Mappers.CRM;

public static class LeadMapper
{
    public static LeadDto ToDto(this Lead entity)
    {
        return new LeadDto
        {
            Id = entity.Id,
            LeadCode = entity.LeadCode,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            CompanyName = entity.CompanyName,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            LeadSource = entity.LeadSource,
            Status = entity.Status,
            Priority = entity.Priority,
            EstimatedValue = entity.EstimatedValue,
            LastContactDate = entity.LastContactDate,
            AssignedToUserId = entity.AssignedToUserId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AssignedToUserName = entity.AssignedToUser?.Username
        };
    }

    public static Lead ToEntity(this LeadDto dto)
    {
        return new Lead
        {
            Id = dto.Id,
            LeadCode = dto.LeadCode,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            LeadSource = dto.LeadSource,
            Status = dto.Status,
            Priority = dto.Priority,
            EstimatedValue = dto.EstimatedValue,
            LastContactDate = dto.LastContactDate,
            AssignedToUserId = dto.AssignedToUserId,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}