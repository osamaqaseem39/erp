using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectDocumentRepository
{
    Task<ProjectDocument?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectDocument>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectDocument>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectDocument>> GetByTypeAsync(string documentType, Guid tenantId);
    Task<Guid> CreateAsync(ProjectDocument projectDocument);
    Task<bool> UpdateAsync(ProjectDocument projectDocument);
    Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
}
