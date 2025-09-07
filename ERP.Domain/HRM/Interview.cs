using ERP.Domain;

namespace ERP.Domain.HRM;

public class Interview : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public Guid InterviewerId { get; set; }
    public DateTime InterviewDate { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int Duration { get; set; }
    public string InterviewType { get; set; } = string.Empty; // Phone, Video, In-Person
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public int Rating { get; set; }
    
    // Navigation properties
    public JobApplication JobApplication { get; set; } = null!;
    public Candidate Candidate { get; set; } = null!;
    public Employee Interviewer { get; set; } = null!;
}
