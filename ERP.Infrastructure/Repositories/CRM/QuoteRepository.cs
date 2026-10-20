using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class QuoteRepository : CrmRepositoryBase, IQuoteRepository
{
    public QuoteRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<Quote> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.Quotes.Where(_ => false);
        return Context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
            .Where(q => q.TenantId == tenantId);
    }

    public async Task<Quote?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quote>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(q => q.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Quote>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(q => q.Status == status).ToListAsync();
    }

    public async Task<Quote?> GetByNumberAsync(string quoteNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber);
    }

    public async Task<IEnumerable<Quote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId)
            .Where(q => q.QuoteDate >= startDate && q.QuoteDate <= endDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Quote quote)
    {
        if (quote.TenantId == Guid.Empty) return Guid.Empty;
        if (quote.Id == Guid.Empty) quote.Id = Guid.NewGuid();
        if (quote.CreatedAt == default) quote.CreatedAt = DateTime.UtcNow;
        Context.Quotes.Add(quote);
        await Context.SaveChangesAsync();
        return quote.Id;
    }

    public async Task<bool> UpdateAsync(Quote quote)
    {
        if (quote.TenantId == Guid.Empty) return false;
        quote.UpdatedAt = DateTime.UtcNow;
        Context.Quotes.Update(quote);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var quote = await ForTenant(tenantId).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return false;
        Context.Quotes.Remove(quote);
        return await Context.SaveChangesAsync() > 0;
    }
}
