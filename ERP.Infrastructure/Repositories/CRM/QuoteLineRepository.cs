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

    public async Task<IEnumerable<QuoteLine>> GetByQuoteIdAsync(Guid quoteId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await Context.QuoteLines
            .Where(l => l.QuoteId == quoteId && l.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(QuoteLine line)
    {
        if (line.TenantId == Guid.Empty) return Guid.Empty;
        if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
        if (line.CreatedAt == default) line.CreatedAt = DateTime.UtcNow;
        Context.QuoteLines.Add(line);
        await Context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteByQuoteIdAsync(Guid quoteId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var lines = await Context.QuoteLines
            .Where(l => l.QuoteId == quoteId && l.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;
        Context.QuoteLines.RemoveRange(lines);
        return await Context.SaveChangesAsync() > 0;
    }
}
