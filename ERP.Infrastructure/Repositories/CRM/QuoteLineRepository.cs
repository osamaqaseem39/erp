using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class QuoteLineRepository : CrmRepositoryBase, IQuoteLineRepository
{
    public QuoteLineRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    public async Task<IEnumerable<QuoteLine>> GetByQuoteIdAsync(Guid quoteId) =>
        await ApplyTenant(Context.QuoteLines.Where(l => l.QuoteId == quoteId)).ToListAsync();

    public async Task<Guid> CreateAsync(QuoteLine line)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            line.TenantId = TenantId.Value;

        Context.QuoteLines.Add(line);
        await Context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteByQuoteIdAsync(Guid quoteId)
    {
        var lines = await ApplyTenant(Context.QuoteLines.Where(l => l.QuoteId == quoteId)).ToListAsync();
        if (lines.Count == 0) return true;

        Context.QuoteLines.RemoveRange(lines);
        return await Context.SaveChangesAsync() > 0;
    }
}
