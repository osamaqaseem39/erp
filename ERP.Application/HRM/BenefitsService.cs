using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class BenefitsService : IBenefitsService
{
    private readonly IBenefitRepository _repo;

    public BenefitsService(IBenefitRepository repo) => _repo = repo;

    public async Task<BenefitDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<BenefitDto>> GetAllAsync(Guid tenantId, string? category = null, bool? isActive = null)
        => (await _repo.GetAllAsync(tenantId, category, isActive)).Select(Map);

    public async Task<BenefitDto> CreateAsync(CreateBenefitDto dto, Guid tenantId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Benefit name is required.");

        var entity = new Benefit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Health" : dto.Type.Trim(),
            Cost = dto.Cost,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString()
        };
        await _repo.CreateAsync(entity);
        return Map(entity);
    }

    public async Task<BenefitDto> UpdateAsync(BenefitDto dto, Guid tenantId, Guid userId)
    {
        var id = dto.Id ?? Guid.Empty;
        var entity = await _repo.GetByIdAsync(id, tenantId)
            ?? throw new KeyNotFoundException($"Benefit '{id}' not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Benefit name is required.");

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.Type = string.IsNullOrWhiteSpace(dto.Type) ? entity.Type : dto.Type.Trim();
        entity.Cost = dto.Cost;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repo.UpdateAsync(entity);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.SoftDeleteAsync(id, tenantId);

    public async Task<IEnumerable<BenefitDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
        => (await _repo.GetByEmployeeAsync(employeeId, tenantId)).Select(Map);

    public async Task<IEnumerable<BenefitDto>> GetActiveAsync(Guid tenantId)
        => (await _repo.GetActiveAsync(tenantId)).Select(Map);

    public async Task<BenefitDto> EnrollEmployeeAsync(Guid benefitId, EnrollEmployeeDto dto, Guid tenantId, Guid userId)
    {
        var benefit = await _repo.GetByIdAsync(benefitId, tenantId)
            ?? throw new KeyNotFoundException($"Benefit '{benefitId}' not found.");
        if (!benefit.IsActive)
            throw new InvalidOperationException("Cannot enroll in an inactive benefit.");

        var employeeId = dto.EmployeeId == Guid.Empty ? throw new ArgumentException("EmployeeId is required.") : dto.EmployeeId;
        var existing = await _repo.GetActiveEnrollmentAsync(benefitId, employeeId, tenantId);
        if (existing != null)
            throw new InvalidOperationException("Employee is already enrolled in this benefit.");

        await _repo.CreateEnrollmentAsync(new EmployeeBenefitEnrollment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BenefitId = benefitId,
            EmployeeId = employeeId,
            EnrollmentDate = dto.EnrollmentDate == default ? DateTime.UtcNow : dto.EnrollmentDate.ToUniversalTime(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
            IsActive = true
        });

        return Map(await _repo.GetByIdAsync(benefitId, tenantId) ?? benefit);
    }

    public async Task<BenefitDto> UnenrollEmployeeAsync(Guid benefitId, UnenrollEmployeeDto dto, Guid tenantId, Guid userId)
    {
        var benefit = await _repo.GetByIdAsync(benefitId, tenantId)
            ?? throw new KeyNotFoundException($"Benefit '{benefitId}' not found.");

        var employeeId = dto.EmployeeId == Guid.Empty ? throw new ArgumentException("EmployeeId is required.") : dto.EmployeeId;
        var enrollment = await _repo.GetActiveEnrollmentAsync(benefitId, employeeId, tenantId)
            ?? throw new InvalidOperationException("No active enrollment found for this employee and benefit.");

        enrollment.Status = "Ended";
        enrollment.UnenrollmentDate = dto.UnenrollmentDate == default
            ? DateTime.UtcNow
            : dto.UnenrollmentDate.ToUniversalTime();
        enrollment.Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim();
        enrollment.UpdatedAt = DateTime.UtcNow;
        enrollment.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repo.UpdateEnrollmentAsync(enrollment);

        return Map(await _repo.GetByIdAsync(benefitId, tenantId) ?? benefit);
    }

    private static BenefitDto Map(Benefit b) => new()
    {
        Id = b.Id,
        TenantId = b.TenantId,
        Name = b.Name,
        Description = b.Description ?? string.Empty,
        Type = b.Type,
        Cost = b.Cost,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        CreatedBy = b.CreatedBy,
        UpdatedBy = b.UpdatedBy
    };
}
