using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Shared.Mappers.CRM;

public static class CampaignMapper
{
    public static CampaignDto ToDto(this Campaign entity)
    {
        return new CampaignDto
        {
            Id = entity.Id,
            CampaignName = entity.CampaignName,
            CampaignType = entity.CampaignType,
            Status = entity.Status,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Budget = entity.Budget,
            ActualCost = entity.ActualCost,
            ExpectedResponse = entity.ExpectedResponse,
            ActualResponse = entity.ActualResponse,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static Campaign ToEntity(this CampaignDto dto)
    {
        return new Campaign
        {
            Id = dto.Id,
            CampaignName = dto.CampaignName,
            CampaignType = dto.CampaignType,
            Status = dto.Status,
            StartDate = dto.StartDate ?? DateTime.MinValue,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            ActualCost = dto.ActualCost,
            ExpectedResponse = dto.ExpectedResponse,
            ActualResponse = dto.ActualResponse,
            Description = dto.Description,
            IsActive = dto.IsActive ?? false,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}