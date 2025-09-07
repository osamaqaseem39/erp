using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectTaskService
{
    Task<ProjectTaskDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectTaskDto>> GetAllAsync();
    Task<IEnumerable<ProjectTaskDto>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectTaskDto>> GetByAssignedToAsync(Guid assignedToId);
    Task<IEnumerable<ProjectTaskDto>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(CreateProjectTaskDto dto);
    Task<bool> UpdateAsync(UpdateProjectTaskDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}