using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class VoidPaymentDto
{
    public Guid PaymentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}