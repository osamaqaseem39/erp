using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class PaymentRepository : IPaymentRepository
{
    private readonly ERPDbContext _context;

    public PaymentRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<Payment> Query()
        => _context.Payments.Include(p => p.PaymentApplications);

    private IQueryable<Payment> ForTenant(Guid tenantId)
        => tenantId == Guid.Empty
            ? Query().Where(_ => false)
            : Query().Where(p => p.TenantId == tenantId);

    public async Task<Payment?> GetByIdAsync(Guid id)
        => await Query().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Payment?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
        => await Query().ToListAsync();

    public async Task<IEnumerable<Payment>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByCustomerAsync(Guid customerId)
        => await Query().Where(p => p.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Payment>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId).Where(p => p.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetBySupplierAsync(Guid supplierId)
        => await Query().Where(p => p.SupplierId == supplierId).ToListAsync();

    public async Task<IEnumerable<Payment>> GetBySupplierAsync(Guid supplierId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId).Where(p => p.SupplierId == supplierId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(string status)
        => await Query().Where(p => p.Status == status).ToListAsync();

    public async Task<IEnumerable<Payment>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId).Where(p => p.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        => await Query()
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .ToListAsync();

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId)
            .Where(p => p.RelatedInvoiceId == invoiceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByBillAsync(Guid billId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Payment>();
        return await ForTenant(tenantId)
            .Where(p => p.RelatedBillId == billId)
            .ToListAsync();
    }

    public async Task<Payment?> GetByNumberAsync(string paymentNumber)
        => await Query().FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);

    public async Task<Payment?> GetByNumberAsync(string paymentNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId)
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    }

    public async Task<Guid> CreateAsync(Payment payment)
    {
        if (payment.TenantId == Guid.Empty) return Guid.Empty;
        if (payment.Id == Guid.Empty) payment.Id = Guid.NewGuid();
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<bool> UpdateAsync(Payment payment)
    {
        if (payment.TenantId == Guid.Empty) return false;
        _context.Payments.Update(payment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return false;
        _context.Payments.Remove(payment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        if (payment == null) return false;
        _context.Payments.Remove(payment);
        return await _context.SaveChangesAsync() > 0;
    }
}
