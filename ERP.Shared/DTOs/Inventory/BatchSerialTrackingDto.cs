using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class BatchSerialTrackingDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public string TrackingType { get; set; } = string.Empty; // Batch, Serial
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid? BatchNumberId { get; set; }
    public string? BatchNumber { get; set; }
    public Guid? SerialNumberId { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class CreateBatchSerialTrackingDto : CreateDto
{
    public Guid ItemId { get; set; }
    public string TrackingType { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid? BatchNumberId { get; set; }
    public Guid? SerialNumberId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class UpdateBatchSerialTrackingDto : UpdateDto
{
    public string? TrackingType { get; set; }
    public string? TrackingNumber { get; set; }
    public Guid? BatchNumberId { get; set; }
    public Guid? SerialNumberId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}