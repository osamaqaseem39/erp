using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using ERP.Infrastructure.Extensions;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
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

    internal static readonly SortMap<Customer> SortFields = new SortMap<Customer>("customerName")
        .Add("customerCode", c => c.CustomerCode)
        .Add("customerName", c => c.CustomerName)
        .Add("companyName", c => c.CompanyName)
        .Add("customerType", c => c.CustomerType)
        .Add("status", c => c.Status)
        .Add("industry", c => c.Industry)
        .Add("email", c => c.Email)
        .Add("creditLimit", c => c.CreditLimit)
        .Add("createdAt", c => c.CreatedAt);

    public async Task<PagedResult<Customer>> GetPagedAsync(CustomerPagedQuery query, CancellationToken cancellationToken = default)
    {
        // List rows need no navigation collections, so skip the Includes used by the detail query.
        var customers = ApplyTenant(Context.Customers.AsNoTracking());

        if (!string.IsNullOrWhiteSpace(query.Status))
            customers = customers.Where(c => c.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Industry))
            customers = customers.Where(c => c.Industry == query.Industry);
        if (!string.IsNullOrWhiteSpace(query.CustomerType))
            customers = customers.Where(c => c.CustomerType == query.CustomerType);

        var term = query.NormalizedSearchTerm;
        if (term != null)
        {
            customers = customers.Where(c =>
                c.CustomerCode.Contains(term) ||
                c.CustomerName.Contains(term) ||
                c.CompanyName.Contains(term) ||
                (c.Email != null && c.Email.Contains(term)) ||
                (c.Phone != null && c.Phone.Contains(term)));
        }

        return await customers.ToPagedResultAsync(query, SortFields, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(string status)
    {
        return await CustomersQuery().Where(c => c.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByIndustryAsync(string industry)
    {
        return await CustomersQuery().Where(c => c.Industry == industry).ToListAsync();
    }

    public async Task<IReadOnlyDictionary<string, int>> GetDependentCountsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Counted separately (not via the Includes on the detail query) so nothing is loaded into the change tracker.
        var counts = new Dictionary<string, int>
        {
            ["contacts"] = await ApplyTenant(Context.Contacts).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["opportunities"] = await ApplyTenant(Context.Opportunities).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["quotes"] = await ApplyTenant(Context.Quotes).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["sales orders"] = await ApplyTenant(Context.SalesOrders).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["activities"] = await ApplyTenant(Context.Activities).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["return orders"] = await ApplyTenant(Context.ReturnOrders).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["projects"] = await ApplyTenant(Context.Projects).CountAsync(x => x.ClientId == id, cancellationToken),
        };
        return counts.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
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
        // Load without navigation collections: with them tracked, EF would null out CustomerId on the
        // customer's opportunities/quotes/orders (ClientSetNull) instead of letting the delete be refused.
        var customer = await ApplyTenant(Context.Customers).FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return false;

        Context.Customers.Remove(customer);
        return await Context.SaveChangesAsync() > 0;
    }
}
