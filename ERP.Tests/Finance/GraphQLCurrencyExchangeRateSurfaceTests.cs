using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLCurrencyExchangeRateSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesCurrencyAndExchangeRateOps()
    {
        var queries = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetCurrency", queries);
        Assert.Contains("GetCurrencies", queries);
        Assert.Contains("CurrencyExists", queries);
        Assert.Contains("GetExchangeRate", queries);
        Assert.Contains("GetExchangeRates", queries);
        Assert.Contains("GetExchangeRatesByCurrency", queries);
        Assert.Contains("GetCurrentExchangeRate", queries);
        Assert.Contains("ConvertAmount", queries);

        Assert.Contains("AddCurrency", mutations);
        Assert.Contains("UpdateCurrency", mutations);
        Assert.Contains("DeleteCurrency", mutations);
        Assert.Contains("AddExchangeRate", mutations);
        Assert.Contains("UpdateExchangeRate", mutations);
        Assert.Contains("DeleteExchangeRate", mutations);
    }

    [Fact]
    public async Task GraphQL_Currency_Create_List_Exists()
    {
        await using var db = Db();
        var svc = new CurrencyService(new CurrencyRepository(db));
        var gql = new FinanceMutations(
            accountingService: null!,
            invoiceService: null!,
            billService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);
        var queries = new FinanceQueries(
            accountingService: null!,
            invoiceService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);

        var created = await gql.AddCurrency(svc, new CreateCurrencyDto
        {
            CurrencyCode = "PKR",
            CurrencyName = "Pakistani Rupee",
            Iso4217 = "PKR",
            Symbol = "Rs"
        });
        Assert.Equal("PKR", created.CurrencyCode);

        Assert.True(await queries.CurrencyExists(svc, "PKR"));
        var listed = (await queries.GetCurrencies(svc)).ToList();
        Assert.Contains(listed, c => c.CurrencyCode == "PKR");

        var updated = await gql.UpdateCurrency(svc, new UpdateCurrencyDto
        {
            CurrencyCode = "PKR",
            CurrencyName = "Rupee"
        });
        Assert.Equal("Rupee", updated.CurrencyName);
    }

    [Fact]
    public async Task GraphQL_ExchangeRate_Create_Current_And_Convert()
    {
        await using var db = Db();
        var currencies = new CurrencyService(new CurrencyRepository(db));
        var rates = new ExchangeRateService(new ExchangeRateRepository(db));
        var gql = new FinanceMutations(
            accountingService: null!,
            invoiceService: null!,
            billService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);
        var queries = new FinanceQueries(
            accountingService: null!,
            invoiceService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);

        await gql.AddCurrency(currencies, new CreateCurrencyDto
        {
            CurrencyCode = "USD", CurrencyName = "US Dollar", Iso4217 = "USD", Symbol = "$"
        });
        await gql.AddCurrency(currencies, new CreateCurrencyDto
        {
            CurrencyCode = "EUR", CurrencyName = "Euro", Iso4217 = "EUR", Symbol = "€"
        });

        var rate = await gql.AddExchangeRate(rates, new CreateExchangeRateDto
        {
            TenantId = Tenant,
            FromCurrency = "USD",
            ToCurrency = "EUR",
            Rate = 0.9m,
            EffectiveDate = DateTime.UtcNow.Date.AddDays(-1)
        });
        Assert.Equal(0.9m, rate.Rate);

        var current = await queries.GetCurrentExchangeRate(rates, "USD", "EUR");
        Assert.NotNull(current);
        Assert.Equal(0.9m, current!.Rate);

        var converted = await queries.ConvertAmount(rates, 100m, "USD", "EUR");
        Assert.Equal(90m, converted);
    }
}
