using ERP.Domain;

namespace ERP.Domain.HRM;

public class Shift : BaseEntity
{
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsOvernight { get; set; }
    public int GracePeriodMinutes { get; set; }
}
