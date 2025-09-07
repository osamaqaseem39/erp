using ERP.Domain.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Shared.Mappers.HRM;

public class TrainingMapper : IMapper<Training, TrainingDto>
{
    public TrainingDto ToDto(Training entity)
    {
        return new TrainingDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Title = entity.Title,
            Description = entity.Description,
            TrainingType = entity.TrainingType,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Duration = (int)(entity.EndDate - entity.StartDate).TotalDays,
            Location = entity.Location,
            Instructor = entity.Instructor,
            MaxParticipants = entity.MaxParticipants,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    public Training ToEntity(TrainingDto dto)
    {
        return new Training
        {
            Id = dto.Id,
            TenantId = dto.TenantId,
            Title = dto.Title,
            Description = dto.Description,
            TrainingType = dto.TrainingType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Location = dto.Location,
            Instructor = dto.Instructor,
            MaxParticipants = dto.MaxParticipants,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy,
            IsActive = dto.IsActive
        };
    }

    public Training MapToEntity(CreateTrainingDto dto)
    {
        return new Training
        {
            TenantId = dto.TenantId,
            Title = dto.Title,
            Description = dto.Description,
            TrainingType = dto.TrainingType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Location = dto.Location,
            Instructor = dto.Instructor,
            MaxParticipants = dto.MaxParticipants,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy
        };
    }

    public void MapToEntity(UpdateTrainingDto dto, Training entity)
    {
        if (!string.IsNullOrEmpty(dto.Title)) entity.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description)) entity.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.TrainingType)) entity.TrainingType = dto.TrainingType;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        if (!string.IsNullOrEmpty(dto.Location)) entity.Location = dto.Location;
        if (dto.InstructorId.HasValue) entity.InstructorId = dto.InstructorId;
        entity.MaxParticipants = dto.MaxParticipants;
        if (!string.IsNullOrEmpty(dto.Status)) entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}