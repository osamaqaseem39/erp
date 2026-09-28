using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class SalesInvoiceRepository : ISalesInvoiceRepository
{
    private readonly ERPDbContext _context;

    public SalesInvoiceRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SalesInvoice?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.SalesInvoices
            .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);
    }

    public async Task<IEnumerable<SalesInvoice>> GetAllAsync(Guid tenantId)
    {
        return await _context.SalesInvoices
            .Where(si => si.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<SalesInvoice> AddAsync(SalesInvoice entity)
    {
        _context.SalesInvoices.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(SalesInvoice entity)
    {
        _context.SalesInvoices.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity != null)
        {
            _context.SalesInvoices.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<SalesInvoice>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        return await _context.SalesInvoices
            .Where(si => si.CustomerId == customerId && si.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesInvoice>> GetByStatusAsync(string status, Guid tenantId)
    {
        return await _context.SalesInvoices
            .Where(si => si.Status == status && si.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        var entity = await GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return false;

        entity.Status = "Posted";
        entity.PostedAt = DateTime.UtcNow;
        entity.PostedBy = postedBy;
        entity.UpdatedBy = postedBy.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid tenantId, Guid voidedBy)
    {
        var entity = await GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return false;

        entity.Status = "Voided";
        entity.UpdatedBy = voidedBy.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}