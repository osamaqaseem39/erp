using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class LeadRepository : CrmRepositoryBase, ILeadRepository
{
    public LeadRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Lead> LeadsQuery() =>
        ApplyTenant(Context.Leads.Include(l => l.Activities));

    public async Task<Lead?> GetByIdAsync(Guid id) =>
        await LeadsQuery().FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<Lead>> GetAllAsync() =>
        await LeadsQuery().ToListAsync();

    public async Task<IEnumerable<Lead>> GetByStatusAsync(string status) =>
        await LeadsQuery().Where(l => l.Status == status).ToListAsync();

    public async Task<IEnumerable<Lead>> GetBySourceAsync(string source) =>
        await LeadsQuery().Where(l => l.Source == source).ToListAsync();

    public async Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedToId) =>
        await LeadsQuery().Where(l => l.AssignedToId == assignedToId || l.AssignedTo == assignedToId).ToListAsync();

    public async Task<IEnumerable<Lead>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.Trim();
        return await LeadsQuery().Where(l =>
            (l.CompanyName != null && l.CompanyName.Contains(term)) ||
            (l.ContactPerson != null && l.ContactPerson.Contains(term)) ||
            (l.Email != null && l.Email.Contains(term)) ||
            l.LeadCode.Contains(term)).ToListAsync();
    }

    public async Task<Guid> CreateAsync(Lead lead)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            lead.TenantId = TenantId.Value;

        Context.Leads.Add(lead);
        await Context.SaveChangesAsync();
        return lead.Id;
    }

    public async Task<bool> UpdateAsync(Lead lead)
    {
        if (!MatchesTenant(lead)) return false;
        Context.Leads.Update(lead);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lead = await GetByIdAsync(id);
        if (lead == null) return false;
        Context.Leads.Remove(lead);
        return await Context.SaveChangesAsync() > 0;
    }
}
