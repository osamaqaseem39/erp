using ERP.Domain;

namespace ERP.Domain.Reports;

public class ReportSchedule : BaseEntity
{
    public Guid ReportId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Yearly
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public new bool IsActive { get; set; } = true;
    public string? EmailRecipients { get; set; } // Comma-separated email addresses
    public string? Notes { get; set; }
    
    // Navigation properties
    public Report Report { get; set; } = null!;
}