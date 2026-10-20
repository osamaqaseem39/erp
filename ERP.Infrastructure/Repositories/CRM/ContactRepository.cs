using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class ContactRepository : CrmRepositoryBase, IContactRepository
{
    public ContactRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Contact> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Contacts.Where(_ => false);
        return Context.Contacts.Where(c => c.TenantId == tenantId);
    }

    private IQueryable<Contact> DetailQuery(Guid tenantId) =>
        ForTenant(tenantId).Include(c => c.Customer);

    public async Task<Contact?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await DetailQuery(tenantId).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Contact>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await DetailQuery(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Contact>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await DetailQuery(tenantId).Where(c => c.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Contact>> SearchAsync(string searchTerm, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var term = searchTerm.Trim();
        return await DetailQuery(tenantId).Where(c =>
            c.FirstName.Contains(term) ||
            c.LastName.Contains(term) ||
            (c.Email != null && c.Email.Contains(term)) ||
            (c.Phone != null && c.Phone.Contains(term))).ToListAsync();
    }

    public async Task<Guid> CreateAsync(Contact contact)
    {
        if (contact.TenantId == Guid.Empty) return Guid.Empty;
        if (contact.Id == Guid.Empty) contact.Id = Guid.NewGuid();
        if (contact.CreatedAt == default) contact.CreatedAt = DateTime.UtcNow;
        Context.Contacts.Add(contact);
        await Context.SaveChangesAsync();
        return contact.Id;
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        if (contact.TenantId == Guid.Empty) return false;
        contact.UpdatedAt = DateTime.UtcNow;
        Context.Contacts.Update(contact);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var contact = await ForTenant(tenantId).FirstOrDefaultAsync(c => c.Id == id);
        if (contact == null) return false;
        Context.Contacts.Remove(contact);
        return await Context.SaveChangesAsync() > 0;
    }
}
