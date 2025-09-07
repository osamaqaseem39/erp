using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectDocumentService
{
    Task<ProjectDocumentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectDocumentDto>> GetAllAsync();
    Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(string documentType);
    Task<Guid> CreateAsync(CreateProjectDocumentDto dto);
    Task<bool> UpdateAsync(UpdateProjectDocumentDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<byte[]> DownloadAsync(Guid id);
}