using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Domain.HRM;

namespace ERP.Application.HRM;

public class PerformanceService : IPerformanceService
{
    private readonly IPerformanceReviewRepository _repository;

    public PerformanceService(IPerformanceReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<PerformanceReviewDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePerformanceReviewDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdatePerformanceReviewDto dto)
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

    public async Task<IEnumerable<PerformanceReviewDto>> GetByEmployeeAsync(Guid employeeId)
    {
        var entities = await _repository.GetByEmployeeAsync(employeeId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetByReviewerAsync(Guid reviewerId)
    {
        var entities = await _repository.GetByReviewerAsync(reviewerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByPeriodAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    private static PerformanceReviewDto MapToDto(PerformanceReview entity)
    {
        return new PerformanceReviewDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            EmployeeId = entity.EmployeeId,
            ReviewerId = entity.ReviewerId,
            ReviewPeriodStart = entity.ReviewPeriodStart,
            ReviewPeriodEnd = entity.ReviewPeriodEnd,
            ReviewDate = entity.ReviewDate,
            OverallRating = entity.OverallRating,
            GoalsAchieved = entity.GoalsAchieved,
            GoalsNotAchieved = entity.GoalsNotAchieved,
            Strengths = entity.Strengths,
            AreasForImprovement = entity.AreasForImprovement,
            Comments = entity.Comments,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static PerformanceReview MapToEntity(CreatePerformanceReviewDto dto)
    {
        return new PerformanceReview
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            EmployeeId = dto.EmployeeId ?? Guid.Empty,
            ReviewerId = dto.ReviewerId ?? Guid.Empty,
            ReviewPeriodStart = dto.ReviewPeriodStart ?? DateTime.UtcNow,
            ReviewPeriodEnd = dto.ReviewPeriodEnd ?? DateTime.UtcNow,
            ReviewDate = dto.ReviewDate ?? DateTime.UtcNow,
            OverallRating = dto.OverallRating ?? 0m,
            GoalsAchieved = dto.GoalsAchieved,
            GoalsNotAchieved = dto.GoalsNotAchieved,
            Strengths = dto.Strengths,
            AreasForImprovement = dto.AreasForImprovement,
            Comments = dto.Comments,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdatePerformanceReviewDto dto, PerformanceReview entity)
    {
        entity.ReviewerId = dto.ReviewerId ?? entity.ReviewerId;
        entity.ReviewPeriodStart = dto.ReviewPeriodStart ?? entity.ReviewPeriodStart;
        entity.ReviewPeriodEnd = dto.ReviewPeriodEnd ?? entity.ReviewPeriodEnd;
        entity.ReviewDate = dto.ReviewDate ?? entity.ReviewDate;
        entity.OverallRating = dto.OverallRating ?? entity.OverallRating;
        entity.GoalsAchieved = dto.GoalsAchieved;
        entity.GoalsNotAchieved = dto.GoalsNotAchieved;
        entity.Strengths = dto.Strengths;
        entity.AreasForImprovement = dto.AreasForImprovement;
        entity.Comments = dto.Comments;
        entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}