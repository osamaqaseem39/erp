using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class LeadRepository : CrmRepositoryBase, ILeadRepository
{
    public LeadRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Lead> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Leads.Where(_ => false);
        return Context.Leads
            .Include(l => l.Activities)
            .Where(l => l.TenantId == tenantId);
    }

    public async Task<Lead?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Lead>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(l => l.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetBySourceAsync(string source, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(l => l.Source == source).ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedToId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId)
            .Where(l => l.AssignedToId == assignedToId || l.AssignedTo == assignedToId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> SearchAsync(string searchTerm, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var term = searchTerm.Trim();
        return await ForTenant(tenantId).Where(l =>
            (l.CompanyName != null && l.CompanyName.Contains(term)) ||
            (l.ContactPerson != null && l.ContactPerson.Contains(term)) ||
            (l.Email != null && l.Email.Contains(term)) ||
            l.LeadCode.Contains(term)).ToListAsync();
    }

    public async Task<Guid> CreateAsync(Lead lead)
    {
        if (lead.TenantId == Guid.Empty) return Guid.Empty;
        if (lead.Id == Guid.Empty) lead.Id = Guid.NewGuid();
        if (lead.CreatedAt == default) lead.CreatedAt = DateTime.UtcNow;
        Context.Leads.Add(lead);
        await Context.SaveChangesAsync();
        return lead.Id;
    }

    public async Task<bool> UpdateAsync(Lead lead)
    {
        if (lead.TenantId == Guid.Empty) return false;
        lead.UpdatedAt = DateTime.UtcNow;
        Context.Leads.Update(lead);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var lead = await ForTenant(tenantId).FirstOrDefaultAsync(l => l.Id == id);
        if (lead == null) return false;
        Context.Leads.Remove(lead);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<IReadOnlyList<LeadDuplicateDto>> FindDuplicatesAsync(string? email, string? phone, Guid? excludeLeadId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var mail = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower();
        var digits = PhoneDigits(phone);
        if (mail == null && digits == null) return [];

        var leads = await Context.Leads.AsNoTracking()
            .Where(l => l.TenantId == tenantId && l.Id != excludeLeadId && l.Status != "Lost"
                        && ((mail != null && l.Email != null && l.Email.ToLower() == mail) || (digits != null && l.Phone != null)))
            .Select(l => new { l.Id, l.LeadCode, l.FirstName, l.LastName, l.CompanyName, l.Email, l.Phone })
            .ToListAsync();
        var customers = await Context.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId
                        && ((mail != null && c.Email != null && c.Email.ToLower() == mail) || (digits != null && (c.Phone != null || c.Phone2 != null))))
            .Select(c => new { c.Id, c.CustomerCode, c.CustomerName, c.Email, c.Phone, c.Phone2 })
            .ToListAsync();
        var contacts = await Context.Contacts.AsNoTracking()
            .Where(c => c.TenantId == tenantId
                        && ((mail != null && c.Email.ToLower() == mail) || (digits != null && c.Phone != null)))
            .Select(c => new { c.Id, c.FirstName, c.LastName, c.Email, c.Phone })
            .ToListAsync();

        bool Hit(string? e, params string?[] phones) =>
            (mail != null && string.Equals(e?.Trim(), mail, StringComparison.OrdinalIgnoreCase))
            || (digits != null && phones.Any(p => PhoneDigits(p) == digits));

        var result = new List<LeadDuplicateDto>();
        result.AddRange(leads.Where(l => Hit(l.Email, l.Phone)).Select(l => new LeadDuplicateDto
        {
            Kind = "Lead", Id = l.Id, Code = l.LeadCode, Email = l.Email, Phone = l.Phone,
            Name = string.IsNullOrWhiteSpace(l.CompanyName) ? $"{l.FirstName} {l.LastName}".Trim() : l.CompanyName!
        }));
        result.AddRange(customers.Where(c => Hit(c.Email, c.Phone, c.Phone2)).Select(c => new LeadDuplicateDto
        {
            Kind = "Customer", Id = c.Id, Code = c.CustomerCode, Name = c.CustomerName, Email = c.Email, Phone = c.Phone
        }));
        result.AddRange(contacts.Where(c => Hit(c.Email, c.Phone)).Select(c => new LeadDuplicateDto
        {
            Kind = "Contact", Id = c.Id, Name = $"{c.FirstName} {c.LastName}".Trim(), Email = c.Email, Phone = c.Phone
        }));
        return result;
    }

    // Compare on the last 9 digits so "+92 300 1234567" and "0300-1234567" match.
    private static string? PhoneDigits(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var d = new string(phone.Where(char.IsDigit).ToArray());
        if (d.Length < 7) return null;
        return d.Length > 9 ? d[^9..] : d;
    }
}
