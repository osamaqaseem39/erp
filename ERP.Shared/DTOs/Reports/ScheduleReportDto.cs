using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Reports;

public class ScheduleReportDto
{
    public Guid ReportId { get; set; }
    public string ScheduleType { get; set; } = string.Empty;
    public string ScheduleExpression { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public string Format { get; set; } = "PDF";
}