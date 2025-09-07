using ERP.Domain;

namespace ERP.Domain.CRM;

public class Contact : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public Guid? CustomerId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}