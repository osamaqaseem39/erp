using ERP.Domain;

namespace ERP.Domain.HRM;

public class Training : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Internal, External, Online
    public string TrainingType { get; set; } = string.Empty; // Technical, Soft Skills, Compliance, etc.
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, In Progress, Completed, Cancelled
    public string? Location { get; set; }
    public string? Instructor { get; set; }
    public string? Trainer { get; set; } // Alternative trainer field
    public decimal? Cost { get; set; }
    public int MaxParticipants { get; set; }
    public string? Prerequisites { get; set; }
    public string? Materials { get; set; }
    public string? ParticipantsJson { get; set; } // JSON string for participants data
    public Guid? EmployeeId { get; set; } // Primary employee/trainer for this training
    public Guid? InstructorId { get; set; } // Instructor ID for this training
    
    // Navigation properties
    public Employee? Employee { get; set; }
    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}

public class TrainingEnrollment : BaseEntity
{
    public Guid TrainingId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string Status { get; set; } = "Enrolled"; // Enrolled, Completed, Dropped
    public decimal? Score { get; set; }
    public string? CertificateUrl { get; set; }
    
    // Navigation properties
    public Training Training { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}