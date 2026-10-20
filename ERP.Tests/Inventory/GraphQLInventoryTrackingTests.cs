using ERP.Api.GraphQL.Inventory;
using ERP.Application.SCM.Services;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLInventoryTrackingTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesSerialBatchReorderAttribute()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetSerialNumber", queries);
        Assert.Contains("GetSerialNumbersByItem", queries);
        Assert.Contains("GetBatchNumber", queries);
        Assert.Contains("GetBatchNumbersByItem", queries);
        Assert.Contains("GetReorderRule", queries);
        Assert.Contains("GetReorderRules", queries);
        Assert.Contains("GetItemAttribute", queries);
        Assert.Contains("GetItemAttributes", queries);

        Assert.Contains("AddSerialNumber", mutations);
        Assert.Contains("UpdateSerialNumber", mutations);
        Assert.Contains("DeleteSerialNumber", mutations);
        Assert.Contains("AddBatchNumber", mutations);
        Assert.Contains("UpdateBatchNumber", mutations);
        Assert.Contains("DeleteBatchNumber", mutations);
        Assert.Contains("AddReorderRule", mutations);
        Assert.Contains("UpdateReorderRule", mutations);
        Assert.Contains("DeleteReorderRule", mutations);
        Assert.Contains("AddItemAttribute", mutations);
        Assert.Contains("UpdateItemAttribute", mutations);
        Assert.Contains("DeleteItemAttribute", mutations);
    }

    [Fact]
    public async Task GraphQL_Serial_Batch_Reorder_Attribute_Crud()
    {
        await using var db = Db();
        db.Items.Add(new ERP.Domain.SCM.Item
        {
            Id = Item,
            TenantId = Tenant,
            ItemCode = "ITM-T",
            ItemName = "Tracked Widget",
            IsActive = true
        });
        db.Warehouses.Add(new ERP.Domain.SCM.Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH-1",
            WarehouseName = "Main",
            Name = "Main",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var mutations = new InventoryMutations();
        var queries = new InventoryQueries();

        var serials = new SerialNumberService(new SerialNumberRepository(db));
        var batches = new BatchNumberService(new BatchNumberRepository(db), new BatchNumberMapper());
        var rules = new ReorderRuleService(new ReorderRuleRepository(db));
        var attrs = new ItemAttributeService(new ItemAttributeRepository(db));

        var serial = await mutations.AddSerialNumber(serials, new SerialNumberDto
        {
            ItemId = Item,
            SerialNumber = "SN-001",
            Status = "Available",
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, serial.Id);
        Assert.Equal("SN-001", (await queries.GetSerialNumber(serials, serial.Id, Tenant))!.SerialNumber);

        serial.Status = "Reserved";
        Assert.Equal("Reserved", (await mutations.UpdateSerialNumber(serials, serial, Tenant, User)).Status);

        var batch = await mutations.AddBatchNumber(batches, new BatchNumberDto
        {
            ItemId = Item,
            BatchNumber = "BATCH-1",
            ManufacturingDate = DateTime.UtcNow.Date.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.Date.AddDays(365),
            Quantity = 100m,
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, batch.Id);
        Assert.Contains(
            await queries.GetBatchNumbersByItem(batches, Item, Tenant),
            b => b.Id == batch.Id);

        batch.Quantity = 80m;
        Assert.Equal(80m, (await mutations.UpdateBatchNumber(batches, batch, Tenant, User)).Quantity);

        var rule = await mutations.AddReorderRule(rules, new ReorderRuleDto
        {
            ItemId = Item,
            WarehouseId = Warehouse,
            MinStockLevel = 10m,
            MaxStockLevel = 100m,
            ReorderPoint = 20m,
            ReorderQuantity = 50m,
            LeadTimeDays = 7,
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, rule.Id);
        Assert.Contains(
            await queries.GetReorderRules(rules, Tenant, Item, Warehouse),
            r => r.Id == rule.Id);

        rule.ReorderQuantity = 60m;
        Assert.Equal(60m, (await mutations.UpdateReorderRule(rules, rule, Tenant, User)).ReorderQuantity);

        var attr = await mutations.AddItemAttribute(attrs, new ItemAttributeDto
        {
            AttributeName = "Color",
            AttributeType = "Text",
            IsRequired = false,
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, attr.Id);
        Assert.Equal("Color", (await queries.GetItemAttribute(attrs, attr.Id, Tenant))!.AttributeName);

        attr.IsRequired = true;
        Assert.True((await mutations.UpdateItemAttribute(attrs, attr, Tenant, User)).IsRequired);

        Assert.True(await mutations.DeleteItemAttribute(attrs, attr.Id, Tenant));
        Assert.True(await mutations.DeleteReorderRule(rules, rule.Id, Tenant));
        Assert.True(await mutations.DeleteBatchNumber(batches, batch.Id, Tenant));
        Assert.True(await mutations.DeleteSerialNumber(serials, serial.Id, Tenant));
        Assert.Null(await queries.GetSerialNumber(serials, serial.Id, Tenant));
    }
}
