using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public static class CrmLineCalculator
{
    public static decimal CalculateLineTotal(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        var gross = quantity * unitPrice;
        return Math.Round(gross * (1 - discountPercent / 100m), 2);
    }

    public static (decimal SubTotal, decimal TaxAmount, decimal TotalAmount) CalculateTotals(
        IEnumerable<(decimal LineTotal, decimal TaxRate)> lines)
    {
        var lineList = lines.ToList();
        var subTotal = lineList.Sum(l => l.LineTotal);
        var taxAmount = lineList.Sum(l => Math.Round(l.LineTotal * l.TaxRate / 100m, 2));
        return (subTotal, taxAmount, subTotal + taxAmount);
    }

    public static string GenerateNumber(string prefix)
        => $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
}
