using ERP.Domain;

namespace ERP.Domain.HRM;

public class EmployeeAddress : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string AddressType { get; set; } = string.Empty; // Home, EmergencyContact, Permanent
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsPrimary { get; set; }
    
    // Navigation property
    public Employee Employee { get; set; } = null!;
}
