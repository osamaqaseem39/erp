using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class ChartOfAccountDto : BaseDto
{
    public Guid? AccountId { get; set; }
    public new Guid? TenantId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? AccountType { get; set; }
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? CurrencyCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPostingAllowed { get; set; }
    public new DateTime? CreatedAt { get; set; }
    public new DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public class CreateChartOfAccountDto : CreateDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public string? CurrencyCode { get; set; }
    public new bool IsActive { get; set; } = true;
    public bool IsPostingAllowed { get; set; } = true;
}

public class UpdateChartOfAccountDto : UpdateDto
{
    public Guid? AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? AccountType { get; set; }
    public Guid? ParentAccountId { get; set; }
    public string? CurrencyCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPostingAllowed { get; set; }
}
