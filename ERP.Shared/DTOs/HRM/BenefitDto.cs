using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class BenefitDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Health";
    public decimal? Cost { get; set; }
    public new bool IsActive { get; set; } = true;
    public new string? CreatedBy { get; set; }
    public new string? UpdatedBy { get; set; }
}

public class CreateBenefitDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Health";
    public decimal? Cost { get; set; }
    public new bool IsActive { get; set; } = true;
}

public class UpdateBenefitDto : UpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Health";
    public decimal? Cost { get; set; }
    public new bool IsActive { get; set; } = true;
}