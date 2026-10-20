using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectDocumentService
{
    Task<ProjectDocumentDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectDocumentDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(string documentType, Guid tenantId);
    Task<Guid> CreateAsync(CreateProjectDocumentDto dto);
    Task<bool> UpdateAsync(UpdateProjectDocumentDto dto, Guid tenantId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<ProjectDocumentDownload?> DownloadAsync(Guid id, Guid tenantId);
    Task<bool> UploadAsync(Guid id, Guid tenantId, string fileName, byte[] content, string? updatedBy = null);
}

public sealed class ProjectDocumentDownload
{
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = "document";
    public string ContentType { get; init; } = "application/octet-stream";
}
