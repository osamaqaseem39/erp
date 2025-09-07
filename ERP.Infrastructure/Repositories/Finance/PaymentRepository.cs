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

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .Where(p => p.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetBySupplierAsync(Guid supplierId)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .Where(p => p.SupplierId == supplierId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(string status)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .ToListAsync();
    }

    public async Task<Payment?> GetByNumberAsync(string paymentNumber)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    }

    public async Task<Guid> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<bool> UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return false;

        _context.Payments.Remove(payment);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}