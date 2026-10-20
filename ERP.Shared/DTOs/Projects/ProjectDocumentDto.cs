using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class ProjectDocumentDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DocumentType { get; set; } = string.Empty; // Contract, Specification, Report, etc.
    public string Version { get; set; } = "1.0";
    public string Status { get; set; } = "Draft";
    public bool IsPublic { get; set; } = false;
    public string? Notes { get; set; }
}

public class CreateProjectDocumentDto : CreateDto
{
    public Guid ProjectId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? Version { get; set; } = "1.0";
    public string? Status { get; set; } = "Draft";
    public bool? IsPublic { get; set; } = false;
    public string? Notes { get; set; }
    /// <summary>Optional base64 payload stored under the tenant document root on create.</summary>
    public string? ContentBase64 { get; set; }
}

public class UpdateProjectDocumentDto : UpdateDto
{
    public string? DocumentName { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public string? DocumentType { get; set; }
    public string? Version { get; set; }
    public string? Status { get; set; }
    public bool? IsPublic { get; set; }
    public string? Notes { get; set; }
}