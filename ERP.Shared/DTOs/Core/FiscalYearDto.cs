using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class FiscalYearDto : BaseDto
{
    public Guid FiscalYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>Active | Closed</summary>
    public string Status { get; set; } = "Active";
}

public class CreateFiscalYearDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>Defaults to Active.</summary>
    public string? Status { get; set; }
}

public class UpdateFiscalYearDto : UpdateDto
{
    public Guid? FiscalYearId { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
}
