using ERP.Domain;

namespace ERP.Domain.Reports;

/// <summary>User favorite for a catalog report (Phase E hub UX).</summary>
public class ReportFavorite : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ReportId { get; set; }
    public string? ReportCode { get; set; }

    public Report? Report { get; set; }
}
