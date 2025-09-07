using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Reports;

public class GenerateReportDto
{
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string Format { get; set; } = "PDF";
}