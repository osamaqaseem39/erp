using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class ProductionRunDto : BaseDto
{
    public Guid? Id { get; set; }
    public Guid ProductionOrderId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
    public string Status { get; set; } = "Running";
    public string? Notes { get; set; }
    public Guid? OperatorId { get; set; }
    public string? OperatorName { get; set; }
}

public class CreateProductionRunDto : CreateDto
{
    public Guid ProductionOrderId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? QuantityProduced { get; set; }
    public decimal? QuantityRejected { get; set; }
    public string? Status { get; set; } = "Running";
    public string? Notes { get; set; }
    public Guid? OperatorId { get; set; }
}

public class UpdateProductionRunDto : UpdateDto
{
    public string RunNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
    public string Status { get; set; } = "Running";
    public string? Notes { get; set; }
    public Guid? OperatorId { get; set; }
}