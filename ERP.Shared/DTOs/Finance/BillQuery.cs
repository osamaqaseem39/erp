using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class BillQuery
{
    public Guid TenantId { get; set; }
    public Guid? VendorId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}