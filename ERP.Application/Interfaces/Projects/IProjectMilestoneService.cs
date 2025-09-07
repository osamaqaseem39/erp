using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectMilestoneService
{
    Task<ProjectMilestoneDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectMilestoneDto>> GetAllAsync();
    Task<IEnumerable<ProjectMilestoneDto>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectMilestoneDto>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(CreateProjectMilestoneDto dto);
    Task<bool> UpdateAsync(UpdateProjectMilestoneDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}