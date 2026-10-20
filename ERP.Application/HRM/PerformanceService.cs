using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class PerformanceService : IPerformanceService
{
    private readonly IPerformanceReviewRepository _repository;

    public PerformanceService(IPerformanceReviewRepository repository) => _repository = repository;

    public async Task<PerformanceReviewDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(MapToDto);

    public async Task<Guid> CreateAsync(CreatePerformanceReviewDto dto, Guid tenantId, Guid userId)
    {
        if (!dto.EmployeeId.HasValue || dto.EmployeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.");
        if (!dto.ReviewerId.HasValue || dto.ReviewerId == Guid.Empty)
            throw new ArgumentException("ReviewerId is required.");

        var start = (dto.ReviewPeriodStart ?? DateTime.UtcNow).ToUniversalTime();
        var end = (dto.ReviewPeriodEnd ?? start).ToUniversalTime();
        if (end < start)
            throw new ArgumentException("ReviewPeriodEnd must be on or after ReviewPeriodStart.");

        var entity = new PerformanceReview
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            EmployeeId = dto.EmployeeId.Value,
            ReviewerId = dto.ReviewerId.Value,
            ReviewPeriodStart = start,
            ReviewPeriodEnd = end,
            ReviewDate = (dto.ReviewDate ?? DateTime.UtcNow).ToUniversalTime(),
            OverallRating = dto.OverallRating ?? 0m,
            GoalsAchieved = dto.GoalsAchieved,
            GoalsNotAchieved = dto.GoalsNotAchieved,
            Strengths = dto.Strengths,
            AreasForImprovement = dto.AreasForImprovement,
            Comments = dto.Comments,
            Status = "Draft",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString()
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdatePerformanceReviewDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft reviews can be updated.");

        if (dto.ReviewerId.HasValue && dto.ReviewerId != Guid.Empty)
            entity.ReviewerId = dto.ReviewerId.Value;
        if (dto.ReviewPeriodStart.HasValue)
            entity.ReviewPeriodStart = dto.ReviewPeriodStart.Value.ToUniversalTime();
        if (dto.ReviewPeriodEnd.HasValue)
            entity.ReviewPeriodEnd = dto.ReviewPeriodEnd.Value.ToUniversalTime();
        if (entity.ReviewPeriodEnd < entity.ReviewPeriodStart)
            throw new ArgumentException("ReviewPeriodEnd must be on or after ReviewPeriodStart.");
        if (dto.ReviewDate.HasValue)
            entity.ReviewDate = dto.ReviewDate.Value.ToUniversalTime();
        if (dto.OverallRating.HasValue) entity.OverallRating = dto.OverallRating.Value;
        if (dto.GoalsAchieved != null) entity.GoalsAchieved = dto.GoalsAchieved;
        if (dto.GoalsNotAchieved != null) entity.GoalsNotAchieved = dto.GoalsNotAchieved;
        if (dto.Strengths != null) entity.Strengths = dto.Strengths;
        if (dto.AreasForImprovement != null) entity.AreasForImprovement = dto.AreasForImprovement;
        if (dto.Comments != null) entity.Comments = dto.Comments;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft reviews can be deleted.");
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId) =>
        (await _repository.GetByEmployeeAsync(employeeId, tenantId)).Select(MapToDto);

    public async Task<IEnumerable<PerformanceReviewDto>> GetByReviewerAsync(Guid reviewerId, Guid tenantId) =>
        (await _repository.GetByReviewerAsync(reviewerId, tenantId)).Select(MapToDto);

    public async Task<IEnumerable<PerformanceReviewDto>> GetByPeriodAsync(
        DateTime startDate, DateTime endDate, Guid tenantId) =>
        (await _repository.GetByPeriodAsync(startDate, endDate, tenantId)).Select(MapToDto);

    public Task<bool> SubmitAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionAsync(id, tenantId, userId, "Draft", "Submitted");

    public Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionAsync(id, tenantId, userId, "Submitted", "Approved");

    public Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionAsync(id, tenantId, userId, "Approved", "Completed");

    private async Task<bool> TransitionAsync(
        Guid id, Guid tenantId, Guid userId, string fromStatus, string toStatus)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, fromStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Review must be {fromStatus} to move to {toStatus}.");

        entity.Status = toStatus;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    private static PerformanceReviewDto MapToDto(PerformanceReview entity) => new()
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
