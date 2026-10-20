using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public static class AccountPurposes
{
    public const string Cash = "Cash";
    public const string BankClearing = "BankClearing";
    public const string AccountsReceivable = "AccountsReceivable";
    public const string AccountsPayable = "AccountsPayable";
    public const string SalesRevenue = "SalesRevenue";
    public const string SalesReturns = "SalesReturns";
    public const string PurchaseReturns = "PurchaseReturns";
    public const string CostOfGoodsSold = "CostOfGoodsSold";
    public const string Inventory = "Inventory";
    public const string InputTax = "InputTax";
    public const string OutputTax = "OutputTax";
    public const string RetainedEarnings = "RetainedEarnings";
    public const string ExchangeGain = "ExchangeGain";
    public const string ExchangeLoss = "ExchangeLoss";
    public const string DepreciationExpense = "DepreciationExpense";
    public const string AccumulatedDepreciation = "AccumulatedDepreciation";

    public static readonly IReadOnlyList<string> All =
    [
        Cash, BankClearing, AccountsReceivable, AccountsPayable,
        SalesRevenue, SalesReturns, PurchaseReturns, CostOfGoodsSold, Inventory,
        InputTax, OutputTax, RetainedEarnings, ExchangeGain, ExchangeLoss,
        DepreciationExpense, AccumulatedDepreciation
    ];
}

public class AccountDefaultDto : BaseDto
{
    public Guid? AccountDefaultId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpsertAccountDefaultDto
{
    public string Purpose { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpsertAccountDefaultsDto
{
    public List<UpsertAccountDefaultDto> Defaults { get; set; } = new();
}

/// <summary>Last-used cash/bank + cost center for typed voucher forms (Default Voucher).</summary>
public class VoucherDefaultsDto
{
    public Guid? CashOrBankAccountId { get; set; }
    public string? CashOrBankAccountCode { get; set; }
    public string? CashOrBankAccountName { get; set; }
    public Guid? CostCenterId { get; set; }
}
