using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectResourceService
{
    Task<ProjectResourceDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectResourceDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectResourceDto>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectResourceDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<Guid> CreateAsync(CreateProjectResourceDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateProjectResourceDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
