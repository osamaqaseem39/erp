using ERP.Application.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Crm;

public class CustomerDeleteTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    [Fact]
    public async Task RefusesToDeleteACustomerWithHistory_AndLeavesTheHistoryIntact()
    {
        var (service, context, customerId) = Create();
        context.Quotes.AddRange(Quote(customerId), Quote(customerId));
        context.Opportunities.Add(new Opportunity { TenantId = Tenant, CustomerId = customerId, OpportunityName = "Deal", Title = "Deal" });
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(customerId, Tenant));

        Assert.Equal("It has 1 opportunity, 2 quotes. Mark it inactive instead to keep that history.", ex.Message);
        Assert.True(await context.Customers.AnyAsync(c => c.Id == customerId));
        Assert.Equal(2, await context.Quotes.CountAsync(q => q.CustomerId == customerId));
    }

    [Fact]
    public async Task DeletesACustomerWithoutHistory()
    {
        var (service, context, customerId) = Create();

        Assert.True(await service.DeleteAsync(customerId, Tenant));
        Assert.False(await context.Customers.AnyAsync(c => c.Id == customerId));
    }

    [Fact]
    public async Task OtherTenantsRecordsDoNotCount()
    {
        var (service, context, customerId) = Create();
        context.Quotes.Add(new Quote { TenantId = OtherTenant, CustomerId = customerId, QuoteNumber = "Q-X" });
        await context.SaveChangesAsync();

        Assert.True(await service.DeleteAsync(customerId, Tenant));
    }

    [Fact]
    public async Task ReportsNotFoundAsFalse()
    {
        var (service, _, _) = Create();

        Assert.False(await service.DeleteAsync(Guid.NewGuid(), Tenant));
    }

    private static Quote Quote(Guid customerId) => new() { TenantId = Tenant, CustomerId = customerId, QuoteNumber = $"Q-{Guid.NewGuid():N}"[..8] };

    private static (CustomerService service, ERPDbContext context, Guid customerId) Create()
    {
        var context = new ERPDbContext(new DbContextOptionsBuilder<ERPDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var customer = new Customer { TenantId = Tenant, CustomerCode = "C-001", CustomerName = "Acme", CompanyName = "Acme", Status = "Active" };
        context.Customers.Add(customer);
        context.SaveChanges();
        var repository = new CustomerRepository(context, new FixedTenantProvider(Tenant));
        return (new CustomerService(repository, new TestCoreSettings()), context, customer.Id);
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? GetTenantId() => tenantId;
    }
}
