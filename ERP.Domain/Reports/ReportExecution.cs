using ERP.Domain;

namespace ERP.Domain.Reports;

public class ReportExecution : BaseEntity
{
    public Guid ReportId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Running"; // Running, Completed, Failed, Cancelled
    public string? Parameters { get; set; } // JSON string of parameters used
    public string? OutputPath { get; set; } // Path to generated report file
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
    public string? ResultData { get; set; }
    public string? ExecutedBy { get; set; }
    
    // Navigation properties
    public Report Report { get; set; } = null!;
    public ReportSchedule? Schedule { get; set; }
}