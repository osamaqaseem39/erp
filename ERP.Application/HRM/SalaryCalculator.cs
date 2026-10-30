using ERP.Domain.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

/// <summary>Turns an employee's salary structure into a payslip: basic → earnings → gross → deductions/tax → net.</summary>
public static class SalaryCalculator
{
    public static readonly string[] Types = ["Basic", "Earning", "Deduction", "Tax"];
    public static readonly string[] CalculationTypes = ["Fixed", "PercentOfBasic", "PercentOfGross"];

    public sealed record Item(PayrollComponent Component, decimal Value);

    public sealed class Result
    {
        public decimal Basic { get; init; }
        public decimal Allowances { get; init; }
        public decimal Gross => Basic + Allowances;
        public decimal Deductions { get; init; }
        public decimal Tax { get; init; }
        public decimal Net => Math.Max(0, Gross - Deductions - Tax);
        public List<PayslipItemDto> Earnings { get; init; } = new();
        public List<PayslipItemDto> DeductionItems { get; init; } = new();
    }

    /// <param name="fallbackBasic">Used when the structure has no Basic component (contract salary or run default).</param>
    /// <param name="periodFactor">Share of the period worked (0–1); prorates Basic and fixed earnings.</param>
    public static Result Calculate(IEnumerable<Item> items, decimal fallbackBasic, decimal periodFactor = 1m)
    {
        var ordered = items.OrderBy(i => i.Component.DisplayOrder).ThenBy(i => i.Component.Name).ToList();
        bool Is(Item i, string t) => string.Equals(i.Component.Type, t, StringComparison.OrdinalIgnoreCase);

        var earnings = new List<PayslipItemDto>();
        var basicItems = ordered.Where(i => Is(i, "Basic")).ToList();
        decimal basic;
        if (basicItems.Count > 0)
        {
            basic = 0;
            foreach (var b in basicItems)
            {
                var amt = R(b.Value * periodFactor);
                basic += amt;
                earnings.Add(new PayslipItemDto { Name = b.Component.Name, Type = "Basic", Amount = amt });
            }
        }
        else
        {
            basic = R(fallbackBasic * periodFactor);
            if (basic > 0) earnings.Add(new PayslipItemDto { Name = "Basic salary", Type = "Basic", Amount = basic });
        }

        decimal allowances = 0;
        foreach (var e in ordered.Where(i => Is(i, "Earning")))
        {
            var amt = Amount(e, basic, 0, periodFactor);
            if (amt == 0) continue;
            allowances += amt;
            earnings.Add(new PayslipItemDto { Name = e.Component.Name, Type = "Earning", Amount = amt });
        }

        var gross = basic + allowances;
        var taxableGross = basic + ordered.Where(i => Is(i, "Earning") && i.Component.IsTaxable)
            .Sum(i => Amount(i, basic, 0, periodFactor));

        var deductionItems = new List<PayslipItemDto>();
        decimal deductions = 0, tax = 0;
        foreach (var d in ordered.Where(i => Is(i, "Deduction") || Is(i, "Tax")))
        {
            var isTax = Is(d, "Tax");
            var amt = Amount(d, basic, isTax ? taxableGross : gross, 1m);
            if (amt == 0) continue;
            if (isTax) tax += amt; else deductions += amt;
            deductionItems.Add(new PayslipItemDto { Name = d.Component.Name, Type = isTax ? "Tax" : "Deduction", Amount = amt });
        }

        return new Result
        {
            Basic = R(basic),
            Allowances = R(allowances),
            Deductions = R(deductions),
            Tax = R(tax),
            Earnings = earnings,
            DeductionItems = deductionItems,
        };
    }

    private static decimal Amount(Item i, decimal basic, decimal gross, decimal periodFactor) =>
        i.Component.CalculationType switch
        {
            var c when c.Equals("PercentOfBasic", StringComparison.OrdinalIgnoreCase) => R(basic * i.Value / 100m),
            var c when c.Equals("PercentOfGross", StringComparison.OrdinalIgnoreCase) => R(gross * i.Value / 100m),
            _ => R(i.Value * periodFactor),
        };

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
