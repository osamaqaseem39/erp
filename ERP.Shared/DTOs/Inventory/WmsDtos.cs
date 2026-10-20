using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class PutawayTaskLineDto : BaseDto
{
    public Guid PutawayTaskId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? SuggestedLocationId { get; set; }
    public Guid? ActualLocationId { get; set; }
    public string? Notes { get; set; }
}

public class PutawayTaskDto : BaseDto
{
    public string TaskNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string ReferenceDocType { get; set; } = "GRN";
    public Guid? ReferenceDocId { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public List<PutawayTaskLineDto> Lines { get; set; } = [];
}

public class CreatePutawayTaskLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? SuggestedLocationId { get; set; }
    public Guid? ActualLocationId { get; set; }
    public string? Notes { get; set; }
}

public class CreatePutawayTaskDto
{
    public Guid WarehouseId { get; set; }
    public string? TaskNumber { get; set; }
    public string ReferenceDocType { get; set; } = "GRN";
    public Guid? ReferenceDocId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePutawayTaskLineDto> Lines { get; set; } = [];
}

public class CompletePutawayLineDto
{
    public Guid LineId { get; set; }
    public Guid ActualLocationId { get; set; }
}

public class CompletePutawayTaskDto
{
    public List<CompletePutawayLineDto>? Lines { get; set; }
}

public class PickListLineDto : BaseDto
{
    public Guid PickListId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityRequested { get; set; }
    public decimal QuantityPicked { get; set; }
    public Guid? FromLocationId { get; set; }
    public string? Notes { get; set; }
}

public class PickListDto : BaseDto
{
    public string PickNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string ReferenceDocType { get; set; } = "DispatchAdvice";
    public Guid? ReferenceDocId { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public List<PickListLineDto> Lines { get; set; } = [];
}

public class CreatePickListLineDto
{
    public Guid ItemId { get; set; }
    public decimal QuantityRequested { get; set; }
    public Guid? FromLocationId { get; set; }
    public string? Notes { get; set; }
}

public class CreatePickListDto
{
    public Guid WarehouseId { get; set; }
    public string? PickNumber { get; set; }
    public string ReferenceDocType { get; set; } = "DispatchAdvice";
    public Guid? ReferenceDocId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePickListLineDto> Lines { get; set; } = [];
}

public class CompletePickLineDto
{
    public Guid LineId { get; set; }
    public Guid FromLocationId { get; set; }
    public decimal QuantityPicked { get; set; }
}

public class CompletePickListDto
{
    public List<CompletePickLineDto>? Lines { get; set; }
}
