using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class PaymentApplicationRepository : IPaymentApplicationRepository
{
    private readonly ERPDbContext _context;

    public PaymentApplicationRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentApplication?> GetByIdAsync(int id)
    {
        return await _context.PaymentApplications.FindAsync(id);
    }

    public async Task<IEnumerable<PaymentApplication>> GetAllAsync()
    {
        return await _context.PaymentApplications.ToListAsync();
    }

    public async Task<PaymentApplication> AddAsync(PaymentApplication entity)
    {
        _context.PaymentApplications.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(PaymentApplication entity)
    {
        _context.PaymentApplications.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.PaymentApplications.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}