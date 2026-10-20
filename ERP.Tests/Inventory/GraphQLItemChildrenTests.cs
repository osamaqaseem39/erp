using ERP.Api.GraphQL.Inventory;
using ERP.Application.SCM.Services;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLItemChildrenTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesVariantPricingBarcode()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetItemVariant", queries);
        Assert.Contains("GetItemVariantsByItem", queries);
        Assert.Contains("GetItemPricing", queries);
        Assert.Contains("GetItemPricingsByItem", queries);
        Assert.Contains("GetBarcode", queries);
        Assert.Contains("GetBarcodes", queries);

        Assert.Contains("AddItemVariant", mutations);
        Assert.Contains("UpdateItemVariant", mutations);
        Assert.Contains("DeleteItemVariant", mutations);
        Assert.Contains("AddItemPricing", mutations);
        Assert.Contains("UpdateItemPricing", mutations);
        Assert.Contains("DeleteItemPricing", mutations);
        Assert.Contains("AddBarcode", mutations);
        Assert.Contains("UpdateBarcode", mutations);
        Assert.Contains("DeleteBarcode", mutations);
    }

    [Fact]
    public async Task GraphQL_Variant_Pricing_Barcode_Crud()
    {
        await using var db = Db();
        db.Items.Add(new ERP.Domain.SCM.Item
        {
            Id = Item,
            TenantId = Tenant,
            ItemCode = "ITM-1",
            ItemName = "Widget",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var mutations = new InventoryMutations();
        var queries = new InventoryQueries();

        var variants = new ItemVariantService(new ItemVariantRepository(db));
        var pricing = new ItemPricingService(new ItemPricingRepository(db));
        var barcodes = new BarcodeService(new BarcodeRepository(db), new BarcodeMapper());

        var variant = await mutations.AddItemVariant(variants, new ItemVariantDto
        {
            ItemId = Item,
            VariantCode = "V-RED",
            VariantName = "Red",
            SKU = "SKU-RED",
            SellingPrice = 25m,
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, variant.Id);
        var loaded = await queries.GetItemVariant(variants, variant.Id, Tenant);
        Assert.NotNull(loaded);
        Assert.Equal("V-RED", loaded!.VariantCode);

        var listed = (await queries.GetItemVariantsByItem(variants, Item, Tenant)).ToList();
        Assert.Contains(listed, v => v.Id == variant.Id);

        variant.VariantName = "Red XL";
        var updated = await mutations.UpdateItemVariant(variants, variant, Tenant, User);
        Assert.Equal("Red XL", updated.VariantName);

        var price = await mutations.AddItemPricing(pricing, new ItemPricingDto
        {
            ItemId = Item,
            UnitPrice = 30m,
            FinalPrice = 30m,
            ValidFrom = DateTime.UtcNow.Date,
            IsActive = true,
            PriceListName = "Retail"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, price.Id);
        Assert.Contains(
            await queries.GetItemPricingsByItem(pricing, Item, Tenant),
            p => p.Id == price.Id);

        var barcode = await mutations.AddBarcode(barcodes, new BarcodeDto
        {
            ItemId = Item,
            VariantId = variant.Id,
            Barcode = "1234567890123",
            BarcodeType = "EAN13",
            IsPrimary = true,
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, barcode.Id);
        Assert.Equal(
            "1234567890123",
            (await queries.GetBarcode(barcodes, barcode.Id, Tenant))!.Barcode);

        Assert.True(await mutations.DeleteBarcode(barcodes, barcode.Id, Tenant));
        Assert.True(await mutations.DeleteItemPricing(pricing, price.Id, Tenant));
        Assert.True(await mutations.DeleteItemVariant(variants, variant.Id, Tenant));
        Assert.Null(await queries.GetItemVariant(variants, variant.Id, Tenant));
    }
}
