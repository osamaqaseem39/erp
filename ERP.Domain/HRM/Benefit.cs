using ERP.Domain;

namespace ERP.Domain.HRM;

public class Benefit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Category: Health, Dental, Retirement, Life, Other.</summary>
    public string Type { get; set; } = "Health";
    public decimal? Cost { get; set; }

    public ICollection<EmployeeBenefitEnrollment> Enrollments { get; set; } = new List<EmployeeBenefitEnrollment>();
}
