using ERP.Application.Interfaces.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Projects;



namespace ERP.Application.Projects;



public class ProjectDocumentService : IProjectDocumentService

{

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)

    {

        "Specification", "Design", "Report", "Contract", "Other"

    };



    private readonly IProjectDocumentRepository _repository;

    private readonly IProjectRepository _projects;

    private readonly IProjectDocumentStorage _storage;



    public ProjectDocumentService(

        IProjectDocumentRepository repository,

        IProjectRepository projects,

        IProjectDocumentStorage storage)

    {

        _repository = repository;

        _projects = projects;

        _storage = storage;

    }



    public async Task<ProjectDocumentDto?> GetByIdAsync(Guid id, Guid tenantId)

    {

        if (tenantId == Guid.Empty) return null;

        var document = await _repository.GetByIdAsync(id, tenantId);

        return document == null ? null : MapToDto(document);

    }



    public async Task<IEnumerable<ProjectDocumentDto>> GetAllAsync(Guid tenantId)

    {

        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectDocumentDto>();

        return (await _repository.GetAllAsync(tenantId)).Select(MapToDto);

    }



    public async Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(Guid projectId, Guid tenantId)

    {

        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectDocumentDto>();

        var project = await _projects.GetByIdAsync(projectId, tenantId);

        if (project == null) return Enumerable.Empty<ProjectDocumentDto>();

        return (await _repository.GetByProjectAsync(projectId, tenantId)).Select(MapToDto);

    }



    public async Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(string documentType, Guid tenantId)

    {

        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectDocumentDto>();

        return (await _repository.GetByTypeAsync(documentType, tenantId)).Select(MapToDto);

    }



    public async Task<Guid> CreateAsync(CreateProjectDocumentDto dto)

    {

        if (dto.ProjectId == Guid.Empty || dto.TenantId == Guid.Empty) return Guid.Empty;

        if (string.IsNullOrWhiteSpace(dto.DocumentName)) return Guid.Empty;

        var project = await _projects.GetByIdAsync(dto.ProjectId, dto.TenantId);

        if (project == null) return Guid.Empty;



        var id = Guid.NewGuid();

        var fileName = string.IsNullOrWhiteSpace(dto.FileName) ? dto.DocumentName.Trim() : dto.FileName.Trim();

        var relativePath = string.Empty;

        var fileSize = dto.FileSize ?? 0L;



        if (!string.IsNullOrWhiteSpace(dto.ContentBase64))

        {

            byte[] bytes;

            try { bytes = Convert.FromBase64String(dto.ContentBase64); }

            catch (FormatException) { return Guid.Empty; }

            if (bytes.Length == 0) return Guid.Empty;

            relativePath = await _storage.SaveAsync(dto.TenantId, id, fileName, bytes);

            fileSize = bytes.Length;

        }



        var documentType = string.IsNullOrWhiteSpace(dto.DocumentType) ? "Other" : dto.DocumentType.Trim();

        if (!AllowedTypes.Contains(documentType)) documentType = "Other";



        var document = new ProjectDocument

        {

            Id = id,

            TenantId = dto.TenantId,

            ProjectId = dto.ProjectId,

            DocumentName = dto.DocumentName.Trim(),

            DocumentType = documentType,

            FilePath = relativePath,

            Description = dto.Description,

            FileSize = fileSize,

            Version = string.IsNullOrWhiteSpace(dto.Version) ? "1.0" : dto.Version!,

            IsPublic = dto.IsPublic ?? false,

            ProjectTaskId = dto.ProjectTaskId,

            ProjectMilestoneId = dto.ProjectMilestoneId,

            CreatedBy = dto.CreatedBy,

            CreatedAt = DateTime.UtcNow,

            IsActive = true

        };



        return await _repository.CreateAsync(document);

    }



    public async Task<bool> UpdateAsync(UpdateProjectDocumentDto dto, Guid tenantId)

    {

        if (tenantId == Guid.Empty) return false;

        var document = await _repository.GetByIdAsync(dto.Id, tenantId);

        if (document == null) return false;



        if (dto.DocumentName != null) document.DocumentName = dto.DocumentName.Trim();

        if (dto.DocumentType != null)

        {

            var t = dto.DocumentType.Trim();

            document.DocumentType = AllowedTypes.Contains(t) ? t : document.DocumentType;

        }

        if (dto.Description != null) document.Description = dto.Description;

        if (dto.Version != null) document.Version = dto.Version;

        if (dto.IsPublic.HasValue) document.IsPublic = dto.IsPublic.Value;

        // FilePath from clients is ignored — only upload/create writes storage paths.

        document.UpdatedBy = dto.UpdatedBy;

        document.UpdatedAt = DateTime.UtcNow;



        return await _repository.UpdateAsync(document);

    }



    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)

    {

        if (tenantId == Guid.Empty) return false;

        var document = await _repository.GetByIdAsync(id, tenantId);

        if (document == null) return false;

        if (!string.IsNullOrWhiteSpace(document.FilePath))

            await _storage.DeleteAsync(tenantId, document.FilePath);

        return await _repository.SoftDeleteAsync(id, tenantId);

    }



    public async Task<ProjectDocumentDownload?> DownloadAsync(Guid id, Guid tenantId)

    {

        if (tenantId == Guid.Empty) return null;

        var document = await _repository.GetByIdAsync(id, tenantId);

        if (document == null || string.IsNullOrWhiteSpace(document.FilePath)) return null;



        var bytes = await _storage.ReadAsync(tenantId, document.FilePath);

        if (bytes == null || bytes.Length == 0) return null;



        var fileName = Path.GetFileName(document.FilePath.Replace('/', Path.DirectorySeparatorChar));

        if (string.IsNullOrWhiteSpace(fileName)) fileName = document.DocumentName;



        return new ProjectDocumentDownload

        {

            Content = bytes,

            FileName = fileName,

            ContentType = GuessContentType(fileName)

        };

    }



    public async Task<bool> UploadAsync(Guid id, Guid tenantId, string fileName, byte[] content, string? updatedBy = null)

    {

        if (tenantId == Guid.Empty || content == null || content.Length == 0) return false;

        var document = await _repository.GetByIdAsync(id, tenantId);

        if (document == null) return false;



        if (!string.IsNullOrWhiteSpace(document.FilePath))

            await _storage.DeleteAsync(tenantId, document.FilePath);



        var safeName = string.IsNullOrWhiteSpace(fileName) ? document.DocumentName : fileName;

        document.FilePath = await _storage.SaveAsync(tenantId, id, safeName, content);

        document.FileSize = content.Length;

        document.UpdatedBy = updatedBy;

        document.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(document);

    }



    private static string GuessContentType(string fileName)

    {

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();

        return ext switch

        {

            ".pdf" => "application/pdf",

            ".png" => "image/png",

            ".jpg" or ".jpeg" => "image/jpeg",

            ".txt" => "text/plain",

            ".csv" => "text/csv",

            ".json" => "application/json",

            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

            _ => "application/octet-stream"

        };

    }



    private static ProjectDocumentDto MapToDto(ProjectDocument document)

    {

        var storedName = string.IsNullOrWhiteSpace(document.FilePath)

            ? string.Empty

            : Path.GetFileName(document.FilePath.Replace('/', Path.DirectorySeparatorChar));

        return new ProjectDocumentDto

        {

            Id = document.Id,

            TenantId = document.TenantId,

            ProjectId = document.ProjectId,

            DocumentName = document.DocumentName,

            DocumentType = document.DocumentType,

            FileName = storedName,

            FilePath = document.FilePath,

            Description = document.Description,

            FileSize = document.FileSize,

            Version = document.Version ?? "1.0",

            IsPublic = document.IsPublic,

            ProjectTaskId = document.ProjectTaskId,

            ProjectMilestoneId = document.ProjectMilestoneId,

            ProjectName = document.Project?.ProjectName,

            CreatedAt = document.CreatedAt,

            UpdatedAt = document.UpdatedAt,

            CreatedBy = document.CreatedBy,

            UpdatedBy = document.UpdatedBy,

            IsActive = document.IsActive

        };

    }

}

