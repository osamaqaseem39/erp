using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class BudgetDto : BaseDto
{
    public string BudgetName { get; set; } = string.Empty;
    public string BudgetType { get; set; } = string.Empty; // Annual, Quarterly, Monthly
    public Guid? GLAccountId { get; set; }
    public string? GLAccountName { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public int FiscalYear { get; set; }
    public int? Quarter { get; set; }
    public int? Month { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
}

public class CreateBudgetDto : CreateDto
{
    public string BudgetName { get; set; } = string.Empty;
    public string BudgetType { get; set; } = string.Empty;
    public Guid? GLAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public int? FiscalYear { get; set; }
    public int? Quarter { get; set; }
    public int? Month { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? Status { get; set; } = "Draft";
    public string? Notes { get; set; }
}

public class UpdateBudgetDto : UpdateDto
{
    public string? BudgetName { get; set; }
    public string? BudgetType { get; set; }
    public Guid? GLAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public int? FiscalYear { get; set; }
    public int? Quarter { get; set; }
    public int? Month { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}