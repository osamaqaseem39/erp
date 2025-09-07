using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Reports;

public class ReportScheduleDto : BaseDto
{
    public Guid? Id { get; set; }
    public Guid ReportId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public new bool IsActive { get; set; } = true;
    public string? EmailRecipients { get; set; }
    public string? Notes { get; set; }
    public string? ReportName { get; set; }
}

public class CreateReportScheduleDto
{
    public Guid ReportId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string? EmailRecipients { get; set; }
    public string? Notes { get; set; }
}

public class UpdateReportScheduleDto
{
    public new Guid Id { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public new bool IsActive { get; set; } = true;
    public string? EmailRecipients { get; set; }
    public string? Notes { get; set; }
}