using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class CustomerRepository : CrmRepositoryBase, ICustomerRepository
{
    public CustomerRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Customer> CustomersQuery() =>
        ApplyTenant(Context.Customers
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders));

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await CustomersQuery().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await CustomersQuery().ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(string status)
    {
        return await CustomersQuery().Where(c => c.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByIndustryAsync(string industry)
    {
        return await CustomersQuery().Where(c => c.Industry == industry).ToListAsync();
    }

    public async Task<Customer?> GetByCodeAsync(string customerCode)
    {
        return await CustomersQuery().FirstOrDefaultAsync(c => c.CustomerCode == customerCode);
    }

    public async Task<Guid> CreateAsync(Customer customer)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            customer.TenantId = TenantId.Value;

        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();
        return customer.Id;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        if (!MatchesTenant(customer)) return false;

        Context.Customers.Update(customer);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null) return false;

        Context.Customers.Remove(customer);
        return await Context.SaveChangesAsync() > 0;
    }
}
