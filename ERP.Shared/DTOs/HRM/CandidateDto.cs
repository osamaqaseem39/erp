using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class CandidateDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ZipCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentCompany { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? ResumePath { get; set; }
    public string? CoverLetter { get; set; }
    public string Status { get; set; } = "Applied";
    public string? Notes { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? Skills { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
}

public class CreateCandidateDto : CreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ZipCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentCompany { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? ResumePath { get; set; }
    public string? CoverLetter { get; set; }
    public string? Status { get; set; } = "Applied";
    public string? Notes { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? Skills { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
}

public class UpdateCandidateDto : UpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ZipCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentCompany { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? ResumePath { get; set; }
    public string? CoverLetter { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? Skills { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
}