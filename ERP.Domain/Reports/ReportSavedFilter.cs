using ERP.Domain;

namespace ERP.Domain.Reports;

/// <summary>Named parameter preset for a catalog report (Phase E hub UX).</summary>
public class ReportSavedFilter : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ReportId { get; set; }
    public string? ReportCode { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>JSON parameter bag (fromDate, toDate, accountId, …).</summary>
    public string ParametersJson { get; set; } = "{}";

    public Report? Report { get; set; }
}
