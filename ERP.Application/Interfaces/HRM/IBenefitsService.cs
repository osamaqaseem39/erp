using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IBenefitsService
{
    Task<BenefitDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BenefitDto>> GetAllAsync(Guid tenantId, string? category = null, bool? isActive = null);
    Task<BenefitDto> CreateAsync(CreateBenefitDto dto, Guid tenantId, Guid userId);
    Task<BenefitDto> UpdateAsync(BenefitDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BenefitDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<BenefitDto>> GetActiveAsync(Guid tenantId);
    Task<BenefitDto> EnrollEmployeeAsync(Guid benefitId, EnrollEmployeeDto dto, Guid tenantId, Guid userId);
    Task<BenefitDto> UnenrollEmployeeAsync(Guid benefitId, UnenrollEmployeeDto dto, Guid tenantId, Guid userId);
}