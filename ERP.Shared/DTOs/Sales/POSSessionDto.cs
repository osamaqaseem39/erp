using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Sales;

public class POSSessionDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public string? CashierName { get; set; }
    public Guid TerminalId { get; set; }
    public string? TerminalName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal ExpectedBalance { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
}

public class CreatePOSSessionDto : CreateDto
{
    public string SessionNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid TerminalId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal OpeningBalance { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
}

public class UpdatePOSSessionDto : UpdateDto
{
    public string SessionNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid TerminalId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
}

public class ClosePOSSessionDto
{
    public Guid SessionId { get; set; }
    public decimal ClosingBalance { get; set; }
    public string? Notes { get; set; }
}