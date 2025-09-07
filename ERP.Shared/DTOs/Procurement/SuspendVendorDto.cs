using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class SuspendVendorDto
{
    public Guid VendorId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}