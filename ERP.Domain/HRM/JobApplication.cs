using ERP.Domain;

namespace ERP.Domain.HRM;

public class JobApplication : BaseEntity
{
    public Guid JobOpeningId { get; set; }
    public Guid CandidateId { get; set; }
    public string ResumePath { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Applied"; // Applied, Shortlisted, Interviewed, Rejected, Hired
    public string CoverLetter { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Navigation properties
    public JobOpening JobOpening { get; set; } = null!;
    public Candidate Candidate { get; set; } = null!;
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}
