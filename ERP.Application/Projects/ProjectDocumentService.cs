using ERP.Application.Interfaces.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;
using ERP.Domain.Projects;

namespace ERP.Application.Projects;

public class ProjectDocumentService : IProjectDocumentService
{
    private readonly IProjectDocumentRepository _projectDocumentRepository;

    public ProjectDocumentService(IProjectDocumentRepository projectDocumentRepository)
    {
        _projectDocumentRepository = projectDocumentRepository;
    }

    public async Task<ProjectDocumentDto?> GetByIdAsync(Guid id)
    {
        var document = await _projectDocumentRepository.GetByIdAsync(id);
        return document != null ? MapToDto(document) : null;
    }

    public async Task<IEnumerable<ProjectDocumentDto>> GetAllAsync()
    {
        var documents = await _projectDocumentRepository.GetAllAsync();
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(Guid projectId)
    {
        var documents = await _projectDocumentRepository.GetByProjectAsync(projectId);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(string documentType)
    {
        var documents = await _projectDocumentRepository.GetByTypeAsync(documentType);
        return documents.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateProjectDocumentDto dto)
    {
        var document = new ProjectDocument
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            DocumentName = dto.DocumentName,
            DocumentType = dto.DocumentType,
            FilePath = dto.FilePath,
            Description = dto.Description,
            FileSize = dto.FileSize ?? 0,
            Version = dto.Version,
            IsPublic = dto.IsPublic ?? false,
            CreatedAt = DateTime.UtcNow
        };

        return await _projectDocumentRepository.CreateAsync(document);
    }

    public async Task<bool> UpdateAsync(UpdateProjectDocumentDto dto)
    {
        var document = await _projectDocumentRepository.GetByIdAsync(dto.Id);
        if (document == null) return false;

        document.DocumentName = dto.DocumentName;
        document.DocumentType = dto.DocumentType;
        document.FilePath = dto.FilePath;
        document.Description = dto.Description;
        document.FileSize = dto.FileSize ?? document.FileSize;
        document.Version = dto.Version;
        if (dto.IsPublic.HasValue) document.IsPublic = dto.IsPublic.Value;
        document.UpdatedAt = DateTime.UtcNow;

        return await _projectDocumentRepository.UpdateAsync(document);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _projectDocumentRepository.DeleteAsync(id);
    }

    public async Task<byte[]> DownloadAsync(Guid id)
    {
        var document = await _projectDocumentRepository.GetByIdAsync(id);
        if (document == null) throw new FileNotFoundException("Document not found");

        // This would typically read the file from the file system
        // For now, return empty array as placeholder
        return Array.Empty<byte>();
    }

    private static ProjectDocumentDto MapToDto(ProjectDocument document)
    {
        return new ProjectDocumentDto
        {
            Id = document.Id,
            ProjectId = document.ProjectId,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType,
            FilePath = document.FilePath,
            Description = document.Description,
            FileSize = document.FileSize,
            Version = document.Version,
            IsPublic = document.IsPublic,
            ProjectName = document.Project?.ProjectName,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            CreatedBy = document.CreatedBy,
            UpdatedBy = document.UpdatedBy
        };
    }
}