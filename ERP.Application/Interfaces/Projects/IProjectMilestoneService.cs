using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectMilestoneService
{
    Task<ProjectMilestoneDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectMilestoneDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectMilestoneDto>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectMilestoneDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(CreateProjectMilestoneDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateProjectMilestoneDto dto, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
