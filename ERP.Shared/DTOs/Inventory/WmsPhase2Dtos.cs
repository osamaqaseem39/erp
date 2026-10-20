using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ReplenishmentTaskLineDto : BaseDto
{
    public Guid ReplenishmentTaskId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? Notes { get; set; }
}

public class ReplenishmentTaskDto : BaseDto
{
    public string TaskNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public List<ReplenishmentTaskLineDto> Lines { get; set; } = [];
}

public class CreateReplenishmentTaskLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? Notes { get; set; }
}

public class CreateReplenishmentTaskDto
{
    public Guid WarehouseId { get; set; }
    public string? TaskNumber { get; set; }
    public string? Notes { get; set; }
    public List<CreateReplenishmentTaskLineDto> Lines { get; set; } = [];
}

public class PackOrderLineDto : BaseDto
{
    public Guid PackOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? PackageCode { get; set; }
    public string? Notes { get; set; }
}

public class PackOrderDto : BaseDto
{
    public string PackNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid? PickListId { get; set; }
    public Guid? DispatchAdviceId { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    public DateTime? PackedAt { get; set; }
    public string? PackedBy { get; set; }
    public List<PackOrderLineDto> Lines { get; set; } = [];
}

public class CreatePackOrderLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? PackageCode { get; set; }
    public string? Notes { get; set; }
}

public class CreatePackOrderDto
{
    public Guid WarehouseId { get; set; }
    public Guid? PickListId { get; set; }
    public Guid? DispatchAdviceId { get; set; }
    public string? PackNumber { get; set; }
    public string? Notes { get; set; }
    public List<CreatePackOrderLineDto> Lines { get; set; } = [];
}

public class ShipmentLineDto : BaseDto
{
    public Guid ShipmentId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class ShipmentDto : BaseDto
{
    public string ShipmentNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid? PackOrderId { get; set; }
    public Guid? DispatchAdviceId { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? ShippedBy { get; set; }
    public List<ShipmentLineDto> Lines { get; set; } = [];
}

public class CreateShipmentLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateShipmentDto
{
    public Guid WarehouseId { get; set; }
    public Guid? PackOrderId { get; set; }
    public Guid? DispatchAdviceId { get; set; }
    public string? ShipmentNumber { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public List<CreateShipmentLineDto> Lines { get; set; } = [];
}

public class ShipShipmentDto
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
}
