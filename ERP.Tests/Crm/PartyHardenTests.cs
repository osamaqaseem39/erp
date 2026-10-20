using ERP.Application.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class PartyHardenTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Salesman_Crud_And_Block_Delete_When_In_Use()
    {
        await using var db = Db();
        var tenant = new FixedTenant();
        var salesmen = new SalesmanService(new SalesmanRepository(db, tenant));
        var customers = new CustomerService(new CustomerRepository(db, tenant));

        var user = Guid.NewGuid();
        var smId = await salesmen.CreateAsync(new CreateSalesmanDto
        {
            SalesmanCode = "SM-1",
            FullName = "Ali Khan",
            Phone = "03001234567",
            Cnic = "42101-1234567-1"
        }, Tenant, user);
        Assert.NotEqual(Guid.Empty, smId);
        Assert.Equal("Ali Khan", (await salesmen.GetByIdAsync(smId, Tenant))!.FullName);

        var custId = await customers.CreateAsync(new CreateCustomerDto
        {
            CustomerCode = "C-1",
            CustomerName = "Acme",
            CompanyName = "Acme Ltd",
            CreditLimit = 5000m,
            CreditDays = 45,
            TaxType = "Filer",
            Ntn = "1234567-8",
            Strn = "S-99",
            Cnic = "42101-9999999-9",
            SalesmanId = smId
        }, Tenant, user);
        Assert.NotEqual(Guid.Empty, custId);

        var cust = await customers.GetByIdAsync(custId, Tenant);
        Assert.Equal(45, cust!.CreditDays);
        Assert.Equal("Filer", cust.TaxType);
        Assert.Equal("1234567-8", cust.Ntn);
        Assert.Equal("S-99", cust.Strn);
        Assert.Equal(smId, cust.SalesmanId);
        Assert.Equal("Ali Khan", cust.SalesmanName);

        Assert.False(await salesmen.DeleteAsync(smId, Tenant)); // in use
        Assert.NotNull(await salesmen.GetByIdAsync(smId, Tenant));

        var row = await db.Customers.FirstAsync(c => c.Id == custId);
        db.Customers.Remove(row);
        await db.SaveChangesAsync();
        Assert.True(await salesmen.DeleteAsync(smId, Tenant));
        Assert.Null(await salesmen.GetByIdAsync(smId, Tenant));
    }

    [Fact]
    public async Task Customer_Keeps_Legacy_TaxId_Synced_From_Ntn()
    {
        await using var db = Db();
        var customers = new CustomerService(new CustomerRepository(db, new FixedTenant()));
        var id = await customers.CreateAsync(new CreateCustomerDto
        {
            CustomerCode = "C-2",
            CustomerName = "Beta",
            CompanyName = "Beta LLC",
            Ntn = "9988776-5"
        }, Tenant, Guid.NewGuid());
        var c = await customers.GetByIdAsync(id, Tenant);
        Assert.Equal("9988776-5", c!.Ntn);
        Assert.Equal("9988776-5", c.TaxId);
    }
}
