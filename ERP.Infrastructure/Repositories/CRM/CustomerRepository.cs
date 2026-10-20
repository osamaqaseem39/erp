using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Extensions;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
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

    private IQueryable<Customer> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Customers.Where(_ => false);
        return Context.Customers.Where(c => c.TenantId == tenantId);
    }

    private IQueryable<Customer> DetailQuery(Guid tenantId) =>
        ForTenant(tenantId)
            .Include(c => c.Salesman)
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders);

    public async Task<Customer?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await DetailQuery(tenantId).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await DetailQuery(tenantId).ToListAsync();
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

    public async Task<PagedResult<Customer>> GetPagedAsync(
        CustomerPagedQuery query,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return new PagedResult<Customer>();

        var customers = ForTenant(tenantId).AsNoTracking();

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

    public async Task<IEnumerable<Customer>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await DetailQuery(tenantId).Where(c => c.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByIndustryAsync(string industry, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await DetailQuery(tenantId).Where(c => c.Industry == industry).ToListAsync();
    }

    public async Task<IReadOnlyDictionary<string, int>> GetDependentCountsAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return new Dictionary<string, int>();

        var counts = new Dictionary<string, int>
        {
            ["contacts"] = await ForTenantContacts(tenantId).CountAsync(x => x.CustomerId == id, cancellationToken),
            ["opportunities"] = await Context.Opportunities.CountAsync(x => x.TenantId == tenantId && x.CustomerId == id, cancellationToken),
            ["quotes"] = await Context.Quotes.CountAsync(x => x.TenantId == tenantId && x.CustomerId == id, cancellationToken),
            ["sales orders"] = await Context.SalesOrders.CountAsync(x => x.TenantId == tenantId && x.CustomerId == id, cancellationToken),
            ["activities"] = await Context.Activities.CountAsync(x => x.TenantId == tenantId && x.CustomerId == id, cancellationToken),
            ["return orders"] = await Context.ReturnOrders.CountAsync(x => x.TenantId == tenantId && x.CustomerId == id, cancellationToken),
            ["projects"] = await Context.Projects.CountAsync(x => x.TenantId == tenantId && x.ClientId == id, cancellationToken),
        };
        return counts.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private IQueryable<Contact> ForTenantContacts(Guid tenantId) =>
        Context.Contacts.Where(c => c.TenantId == tenantId);

    public async Task<Customer?> GetByCodeAsync(string customerCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await DetailQuery(tenantId).FirstOrDefaultAsync(c => c.CustomerCode == customerCode);
    }

    public async Task<Guid> CreateAsync(Customer customer)
    {
        if (customer.TenantId == Guid.Empty) return Guid.Empty;
        if (customer.Id == Guid.Empty) customer.Id = Guid.NewGuid();
        if (customer.CreatedAt == default) customer.CreatedAt = DateTime.UtcNow;
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();
        return customer.Id;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        if (customer.TenantId == Guid.Empty) return false;
        customer.UpdatedAt = DateTime.UtcNow;
        Context.Customers.Update(customer);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        // Load without navigation collections: with them tracked, EF would null out CustomerId on the
        // customer's opportunities/quotes/orders (ClientSetNull) instead of letting the delete be refused.
        var customer = await ForTenant(tenantId).FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return false;

        Context.Customers.Remove(customer);
        return await Context.SaveChangesAsync() > 0;
    }
}
