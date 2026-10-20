using ERP.Domain;

namespace ERP.Domain.CRM;

public class Salesman : BaseEntity
{
    public string SalesmanCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Cnic { get; set; }
    public string? Remarks { get; set; }
    /// <summary>Active | Inactive</summary>
    public string Status { get; set; } = "Active";
}
