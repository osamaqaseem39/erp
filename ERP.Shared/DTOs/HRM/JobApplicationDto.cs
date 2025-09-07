using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class JobApplicationDto : BaseDto
{
    public Guid JobOpeningId { get; set; }
    public Guid CandidateId { get; set; }
    public string Status { get; set; } = "Applied";
    public DateTime AppliedDate { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public string ResumePath { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CreateJobApplicationDto : CreateDto
{
    public Guid JobOpeningId { get; set; }
    public Guid CandidateId { get; set; }
    public string Status { get; set; } = "Applied";
    public DateTime AppliedDate { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public string ResumePath { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateJobApplicationDto : UpdateDto
{
    public Guid? JobOpeningId { get; set; }
    public Guid? CandidateId { get; set; }
    public string? Status { get; set; }
    public DateTime? AppliedDate { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumePath { get; set; }
    public string? Notes { get; set; }
}