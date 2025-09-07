using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class LeadRepository : ILeadRepository
{
    private readonly ERPDbContext _context;

    public LeadRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id)
    {
        return await _context.Leads
            .Include(l => l.Activities)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Lead>> GetAllAsync()
    {
        return await _context.Leads
            .Include(l => l.Activities)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetByStatusAsync(string status)
    {
        return await _context.Leads
            .Include(l => l.Activities)
            .Where(l => l.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetBySourceAsync(string source)
    {
        return await _context.Leads
            .Include(l => l.Activities)
            .Where(l => l.Source == source)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedToId)
    {
        return await _context.Leads
            .Include(l => l.Activities)
            .Where(l => l.AssignedToId == assignedToId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Lead lead)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();
        return lead.Id;
    }

    public async Task<bool> UpdateAsync(Lead lead)
    {
        _context.Leads.Update(lead);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return false;

        _context.Leads.Remove(lead);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}