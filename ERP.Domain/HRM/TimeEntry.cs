using ERP.Domain;

namespace ERP.Domain.HRM;

public class TimeEntry : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public DateTime Date { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Device { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Approved
    public string? Source { get; set; } // Manual, Biometric, Mobile App, etc.
    
    // Navigation properties
    public Employee Employee { get; set; } = null!;
}