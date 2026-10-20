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
}
