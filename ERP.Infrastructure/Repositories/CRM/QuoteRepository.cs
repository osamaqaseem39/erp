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

    private IQueryable<Quote> QuotesQuery() =>
        ApplyTenant(Context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines));

    public async Task<Quote?> GetByIdAsync(Guid id) =>
        await QuotesQuery().FirstOrDefaultAsync(q => q.Id == id);

    public async Task<IEnumerable<Quote>> GetAllAsync() =>
        await QuotesQuery().ToListAsync();

    public async Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId) =>
        await QuotesQuery().Where(q => q.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Quote>> GetByStatusAsync(string status) =>
        await QuotesQuery().Where(q => q.Status == status).ToListAsync();

    public async Task<Quote?> GetByNumberAsync(string quoteNumber) =>
        await QuotesQuery().FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber);

    public async Task<IEnumerable<Quote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        await QuotesQuery().Where(q => q.QuoteDate >= startDate && q.QuoteDate <= endDate).ToListAsync();

    public async Task<Guid> CreateAsync(Quote quote)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            quote.TenantId = TenantId.Value;

        Context.Quotes.Add(quote);
        await Context.SaveChangesAsync();
        return quote.Id;
    }

    public async Task<bool> UpdateAsync(Quote quote)
    {
        if (!MatchesTenant(quote)) return false;
        Context.Quotes.Update(quote);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var quote = await GetByIdAsync(id);
        if (quote == null) return false;
        Context.Quotes.Remove(quote);
        return await Context.SaveChangesAsync() > 0;
    }
}
