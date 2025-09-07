using ERP.Domain;

namespace ERP.Domain.HRM;

public class EmployeeDocument : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // ID, Contract, Certificate
    public string FileUrl { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsSensitive { get; set; }
    
    // Navigation property
    public Employee Employee { get; set; } = null!;
}
