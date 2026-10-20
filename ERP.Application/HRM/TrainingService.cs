using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class TrainingService : ITrainingService
{
    private readonly ITrainingRepository _repository;

    public TrainingService(ITrainingRepository repository) => _repository = repository;

    public async Task<TrainingDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<TrainingDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(MapToDto);

    public async Task<Guid> CreateAsync(CreateTrainingDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("EndDate must be on or after StartDate.");

        var entity = new Training
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            TrainingType = dto.TrainingType?.Trim() ?? string.Empty,
            Type = string.IsNullOrWhiteSpace(dto.TrainingType) ? "Internal" : dto.TrainingType.Trim(),
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            Location = dto.Location,
            Instructor = dto.Instructor,
            InstructorId = dto.InstructorId,
            Cost = dto.Cost,
            MaxParticipants = dto.MaxParticipants,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString()
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateTrainingDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot update a completed or cancelled training.");

        if (!string.IsNullOrWhiteSpace(dto.Title)) entity.Title = dto.Title.Trim();
        if (dto.Description != null) entity.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.TrainingType)) entity.TrainingType = dto.TrainingType.Trim();
        entity.StartDate = dto.StartDate.ToUniversalTime();
        entity.EndDate = dto.EndDate.ToUniversalTime();
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.Instructor != null) entity.Instructor = dto.Instructor;
        if (dto.InstructorId.HasValue) entity.InstructorId = dto.InstructorId;
        entity.Cost = dto.Cost;
        entity.MaxParticipants = dto.MaxParticipants;
        // Status only via complete/cancel workflow
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repository.DeleteAsync(id, tenantId);

    public async Task<IEnumerable<TrainingDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId) =>
        (await _repository.GetByEmployeeAsync(employeeId, tenantId)).Select(MapToDto);

    public async Task<IEnumerable<TrainingDto>> GetByStatusAsync(string status, Guid tenantId) =>
        (await _repository.GetByStatusAsync(status, tenantId)).Select(MapToDto);

    public async Task<bool> EnrollEmployeeAsync(Guid trainingId, Guid employeeId, Guid tenantId, Guid userId)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("EmployeeId is required.");

        var training = await _repository.GetByIdAsync(trainingId, tenantId);
        if (training == null) return false;
        if (string.Equals(training.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(training.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot enroll in a completed or cancelled training.");

        var existing = await _repository.GetEnrollmentAsync(trainingId, employeeId, tenantId);
        if (existing != null)
            throw new InvalidOperationException("Employee is already enrolled in this training.");

        if (training.MaxParticipants > 0)
        {
            var count = await _repository.CountActiveEnrollmentsAsync(trainingId, tenantId);
            if (count >= training.MaxParticipants)
                throw new InvalidOperationException("Training is at maximum capacity.");
        }

        await _repository.CreateEnrollmentAsync(new TrainingEnrollment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingId = trainingId,
            EmployeeId = employeeId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Enrolled",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
            IsActive = true
        });

        if (string.Equals(training.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
        {
            training.Status = "In Progress";
            training.UpdatedAt = DateTime.UtcNow;
            training.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
            await _repository.UpdateAsync(training);
        }

        return true;
    }

    public async Task<bool> CompleteTrainingAsync(
        Guid trainingId, Guid employeeId, decimal score, Guid tenantId, Guid userId)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("EmployeeId is required.");

        var training = await _repository.GetByIdAsync(trainingId, tenantId);
        if (training == null) return false;

        var enrollment = await _repository.GetEnrollmentAsync(trainingId, employeeId, tenantId);
        if (enrollment == null)
            throw new InvalidOperationException("Employee is not enrolled in this training.");
        if (string.Equals(enrollment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Training already completed for this employee.");

        enrollment.Status = "Completed";
        enrollment.Score = score;
        enrollment.UpdatedAt = DateTime.UtcNow;
        enrollment.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repository.UpdateEnrollmentAsync(enrollment);

        var refreshed = await _repository.GetByIdAsync(trainingId, tenantId);
        var openCount = refreshed?.Enrollments.Count(e => e.Status == "Enrolled") ?? 0;
        if (openCount == 0 && (refreshed?.Enrollments.Any(e => e.Status == "Completed") ?? false))
        {
            training.Status = "Completed";
            training.UpdatedAt = DateTime.UtcNow;
            training.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
            await _repository.UpdateAsync(training);
        }

        return true;
    }

    private static TrainingDto MapToDto(Training entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        Title = entity.Title,
        Description = entity.Description,
        TrainingType = entity.TrainingType,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Duration = Math.Max(0, (int)(entity.EndDate.Date - entity.StartDate.Date).TotalDays),
        Location = entity.Location ?? string.Empty,
        Instructor = entity.Instructor,
        InstructorId = entity.InstructorId,
        Cost = entity.Cost ?? 0,
        MaxParticipants = entity.MaxParticipants,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive
    };
}
