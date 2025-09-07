using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectDocumentRepository
{
    Task<ProjectDocument?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectDocument>> GetAllAsync();
    Task<IEnumerable<ProjectDocument>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectDocument>> GetByTypeAsync(string documentType);
    Task<Guid> CreateAsync(ProjectDocument projectDocument);
    Task<bool> UpdateAsync(ProjectDocument projectDocument);
    Task<bool> DeleteAsync(Guid id);
}