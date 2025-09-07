using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly ERPDbContext _context;

    public OpportunityRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Opportunity?> GetByIdAsync(Guid id)
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Opportunity>> GetAllAsync()
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByStatusAsync(string status)
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByStageAsync(string stage)
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .Where(o => o.Stage == stage)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Opportunity opportunity)
    {
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        return opportunity.Id;
    }

    public async Task<bool> UpdateAsync(Opportunity opportunity)
    {
        _context.Opportunities.Update(opportunity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var opportunity = await _context.Opportunities.FindAsync(id);
        if (opportunity == null) return false;

        _context.Opportunities.Remove(opportunity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Opportunities.AnyAsync(o => o.Id == id);
    }

    public async Task<Opportunity?> GetByCodeAsync(string code)
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .FirstOrDefaultAsync(o => o.OpportunityCode == code);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var query = _context.Opportunities.Where(o => o.OpportunityCode == code);
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo)
    {
        return await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Activities)
            .Where(o => o.AssignedTo == assignedTo)
            .ToListAsync();
    }
}