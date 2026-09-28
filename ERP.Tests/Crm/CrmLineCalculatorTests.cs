using ERP.Application.CRM;

namespace ERP.Tests.Crm;

public class CrmLineCalculatorTests
{
    [Fact]
    public void CalculateLineTotal_AppliesDiscount()
    {
        var total = CrmLineCalculator.CalculateLineTotal(10, 100, 10);
        Assert.Equal(900, total);
    }

    [Fact]
    public void CalculateTotals_SumsLinesAndTax()
    {
        var totals = CrmLineCalculator.CalculateTotals(new[]
        {
            (100m, 10m),
            (200m, 5m)
        });

        Assert.Equal(300m, totals.SubTotal);
        Assert.Equal(20m, totals.TaxAmount);
        Assert.Equal(320m, totals.TotalAmount);
    }

    [Fact]
    public void GenerateNumber_HasPrefix()
    {
        var number = CrmLineCalculator.GenerateNumber("QT");
        Assert.StartsWith("QT-", number);
    }
}
