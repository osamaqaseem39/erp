using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Reports;

public class ReportExecutionDto : BaseDto
{
    public Guid? Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Running";
    public string? Parameters { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
    public string? ReportName { get; set; }
    public string? ScheduleName { get; set; }
    public string? ResultData { get; set; }
    public string? ExecutedBy { get; set; }
}

public class CreateReportExecutionDto
{
    public Guid ReportId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string? Parameters { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
}

public class UpdateReportExecutionDto
{
    public new Guid Id { get; set; }
    public string Status { get; set; } = "Running";
    public string? Parameters { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
}