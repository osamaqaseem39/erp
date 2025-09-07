using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Domain.HRM;

namespace ERP.Application.HRM;

public class TrainingService : ITrainingService
{
    private readonly ITrainingRepository _repository;

    public TrainingService(ITrainingRepository repository)
    {
        _repository = repository;
    }

    public async Task<TrainingDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<TrainingDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateTrainingDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateTrainingDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<TrainingDto>> GetByEmployeeAsync(Guid employeeId)
    {
        var entities = await _repository.GetByEmployeeAsync(employeeId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<TrainingDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<bool> EnrollEmployeeAsync(Guid trainingId, Guid employeeId)
    {
        var entity = await _repository.GetByIdAsync(trainingId);
        if (entity == null) return false;

        // Add employee to training participants
        // This would typically involve updating a many-to-many relationship
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CompleteTrainingAsync(Guid trainingId, Guid employeeId, decimal score)
    {
        var entity = await _repository.GetByIdAsync(trainingId);
        if (entity == null) return false;

        // Update training completion status
        // This would typically involve updating a training completion record
        return await _repository.UpdateAsync(entity);
    }

    private static TrainingDto MapToDto(Training entity)
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

    private static Training MapToEntity(CreateTrainingDto dto)
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

    private static void MapToEntity(UpdateTrainingDto dto, Training entity)
    {
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.TrainingType = dto.TrainingType;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Location = dto.Location;
        entity.Instructor = dto.Instructor;
        entity.MaxParticipants = dto.MaxParticipants;
        entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}