using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class RefundDto
{
    public Guid PaymentId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}