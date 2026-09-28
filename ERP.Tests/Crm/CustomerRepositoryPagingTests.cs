using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Crm;

public class CustomerRepositoryPagingTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    [Fact]
    public async Task OnlyReturnsTheCurrentTenantsCustomers()
    {
        var repository = CreateRepository(TenantA, out _);

        var page = await repository.GetPagedAsync(new CustomerPagedQuery { PageSize = 50 });

        Assert.Equal(3, page.TotalCount);
        Assert.All(page.Items, c => Assert.Equal(TenantA, c.TenantId));
    }

    [Fact]
    public async Task FiltersByStatusAndSearchesAcrossFields()
    {
        var repository = CreateRepository(TenantA, out _);

        var active = await repository.GetPagedAsync(new CustomerPagedQuery { Status = "Active" });
        var byEmail = await repository.GetPagedAsync(new CustomerPagedQuery { SearchTerm = "  globex.io " });

        Assert.Equal(new[] { "Acme", "Globex" }, active.Items.Select(c => c.CustomerName));
        Assert.Equal("Globex", Assert.Single(byEmail.Items).CustomerName);
    }

    [Fact]
    public async Task SortsByWhitelistedField()
    {
        var repository = CreateRepository(TenantA, out _);

        var page = await repository.GetPagedAsync(new CustomerPagedQuery { SortBy = "customerCode", SortDescending = true });

        Assert.Equal(new[] { "C-003", "C-002", "C-001" }, page.Items.Select(c => c.CustomerCode));
    }

    private static CustomerRepository CreateRepository(Guid tenantId, out ERPDbContext context)
    {
        context = new ERPDbContext(new DbContextOptionsBuilder<ERPDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        context.Customers.AddRange(
            Customer(TenantA, "C-001", "Acme", "Active", "sales@acme.test"),
            Customer(TenantA, "C-002", "Globex", "Active", "hello@globex.io"),
            Customer(TenantA, "C-003", "Initech", "Suspended", null),
            Customer(TenantB, "C-001", "Other tenant", "Active", "x@globex.io"));
        context.SaveChanges();
        return new CustomerRepository(context, new FixedTenantProvider(tenantId));
    }

    private static Customer Customer(Guid tenantId, string code, string name, string status, string? email) => new()
    {
        TenantId = tenantId,
        CustomerCode = code,
        CustomerName = name,
        CompanyName = name,
        Status = status,
        Email = email,
    };

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? GetTenantId() => tenantId;
    }
}
