using ERP.Domain;

namespace ERP.Domain.HRM;

public class Candidate : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentCompany { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Skills { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Hired, Rejected
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}
