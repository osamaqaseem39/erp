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

    private IQueryable<Contact> ContactsQuery() =>
        ApplyTenant(Context.Contacts.Include(c => c.Customer));

    public async Task<Contact?> GetByIdAsync(Guid id) =>
        await ContactsQuery().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Contact>> GetAllAsync() =>
        await ContactsQuery().ToListAsync();

    public async Task<IEnumerable<Contact>> GetByCustomerAsync(Guid customerId) =>
        await ContactsQuery().Where(c => c.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Contact>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.Trim();
        return await ContactsQuery().Where(c =>
            c.FirstName.Contains(term) ||
            c.LastName.Contains(term) ||
            (c.Email != null && c.Email.Contains(term)) ||
            (c.Phone != null && c.Phone.Contains(term))).ToListAsync();
    }

    public async Task<Guid> CreateAsync(Contact contact)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            contact.TenantId = TenantId.Value;

        Context.Contacts.Add(contact);
        await Context.SaveChangesAsync();
        return contact.Id;
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        if (!MatchesTenant(contact)) return false;
        Context.Contacts.Update(contact);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var contact = await GetByIdAsync(id);
        if (contact == null) return false;
        Context.Contacts.Remove(contact);
        return await Context.SaveChangesAsync() > 0;
    }
}
