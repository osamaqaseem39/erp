using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class TrainingDto : BaseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? InstructorId { get; set; }
    public string? InstructorName { get; set; }
    public string? Instructor { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int MaxParticipants { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTrainingDto : CreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public string Status { get; set; } = "Scheduled";
    public Guid? InstructorId { get; set; }
    public string? Instructor { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int MaxParticipants { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTrainingDto : UpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public string? Status { get; set; }
    public Guid? InstructorId { get; set; }
    public string? Instructor { get; set; }
    public string? Location { get; set; }
    public decimal Cost { get; set; }
    public int MaxParticipants { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}