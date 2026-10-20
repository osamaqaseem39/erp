namespace ERP.Application.CRM;

public static class CrmLineCalculator
{
    public static decimal CalculateLineTotal(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        var gross = quantity * unitPrice;
        return Math.Round(gross * (1 - discountPercent / 100m), 2);
    }

    /// <summary>Both taxes apply independently on the discounted net (reference Tax1+Tax2).</summary>
    public static (decimal Tax1Amount, decimal Tax2Amount, decimal TaxTotal) CalculateLineTaxes(
        decimal netBeforeTax, decimal tax1Rate, decimal tax2Rate)
    {
        var tax1 = Math.Round(netBeforeTax * tax1Rate / 100m, 2);
        var tax2 = Math.Round(netBeforeTax * tax2Rate / 100m, 2);
        return (tax1, tax2, tax1 + tax2);
    }

    public static (decimal SubTotal, decimal TaxAmount, decimal TotalAmount, decimal Tax1Amount, decimal Tax2Amount)
        CalculateTotals(IEnumerable<(decimal LineTotal, decimal Tax1Rate, decimal Tax2Rate)> lines)
    {
        var lineList = lines.ToList();
        var subTotal = Math.Round(lineList.Sum(l => l.LineTotal), 2);
        decimal tax1 = 0, tax2 = 0;
        foreach (var l in lineList)
        {
            var t = CalculateLineTaxes(l.LineTotal, l.Tax1Rate, l.Tax2Rate);
            tax1 += t.Tax1Amount;
            tax2 += t.Tax2Amount;
        }
        tax1 = Math.Round(tax1, 2);
        tax2 = Math.Round(tax2, 2);
        var taxTotal = tax1 + tax2;
        return (subTotal, taxTotal, subTotal + taxTotal, tax1, tax2);
    }

    /// <summary>Legacy single-tax helper (TaxRate = Tax1).</summary>
    public static (decimal SubTotal, decimal TaxAmount, decimal TotalAmount) CalculateTotals(
        IEnumerable<(decimal LineTotal, decimal TaxRate)> lines)
    {
        var dual = CalculateTotals(lines.Select(l => (l.LineTotal, l.TaxRate, 0m)));
        return (dual.SubTotal, dual.TaxAmount, dual.TotalAmount);
    }

    public static string GenerateNumber(string prefix)
        => $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
}
