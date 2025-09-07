namespace ERP.Domain.Sales;

public class POSSession : BaseEntity
{
    public string SessionNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid TerminalId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal ExpectedBalance { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
}