using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class EmployeeDocumentDto : BaseDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // ID, Passport, Contract, Certificate, etc.
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class CreateEmployeeDocumentDto : CreateDto
{
    public Guid EmployeeId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class UpdateEmployeeDocumentDto : UpdateDto
{
    public string? DocumentName { get; set; }
    public string? DocumentType { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}