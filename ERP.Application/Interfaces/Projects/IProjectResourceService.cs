using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectResourceService
{
    Task<ProjectResourceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectResourceDto>> GetAllAsync();
    Task<IEnumerable<ProjectResourceDto>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectResourceDto>> GetByEmployeeAsync(Guid employeeId);
    Task<Guid> CreateAsync(CreateProjectResourceDto dto);
    Task<bool> UpdateAsync(UpdateProjectResourceDto dto);
    Task<bool> DeleteAsync(Guid id);
}