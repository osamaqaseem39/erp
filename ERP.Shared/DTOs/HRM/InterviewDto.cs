using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class InterviewDto : BaseDto
{
    public Guid JobApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public Guid InterviewerId { get; set; }
    public DateTime InterviewDate { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int Duration { get; set; }
    public string InterviewType { get; set; } = "InPerson";
    public string Status { get; set; } = "Scheduled";
    public string Feedback { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class CreateInterviewDto : CreateDto
{
    public Guid JobApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public Guid InterviewerId { get; set; }
    public DateTime InterviewDate { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int Duration { get; set; }
    public string InterviewType { get; set; } = "InPerson";
    public string Status { get; set; } = "Scheduled";
    public string Feedback { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInterviewDto : UpdateDto
{
    public Guid? JobApplicationId { get; set; }
    public Guid? CandidateId { get; set; }
    public Guid? InterviewerId { get; set; }
    public DateTime? InterviewDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public int? Duration { get; set; }
    public string? InterviewType { get; set; }
    public string? Status { get; set; }
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class CompleteInterviewDto
{
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}