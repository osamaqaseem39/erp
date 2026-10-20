using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectMember>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectMember>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectMember>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<Guid> CreateAsync(ProjectMember member);
    Task<bool> UpdateAsync(ProjectMember member);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
