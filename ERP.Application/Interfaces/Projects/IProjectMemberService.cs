using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectMemberService
{
    Task<ProjectMemberDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectMemberDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectMemberDto>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectMemberDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<Guid> CreateAsync(CreateProjectMemberDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateProjectMemberDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
