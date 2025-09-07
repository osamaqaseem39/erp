using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class VendorBankAccountRepository : IVendorBankAccountRepository
{
    private readonly ERPDbContext _context;

    public VendorBankAccountRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<VendorBankAccount?> GetByIdAsync(int id)
    {
        return await _context.VendorBankAccounts.FindAsync(id);
    }

    public async Task<IEnumerable<VendorBankAccount>> GetAllAsync()
    {
        return await _context.VendorBankAccounts.ToListAsync();
    }

    public async Task<VendorBankAccount> AddAsync(VendorBankAccount entity)
    {
        _context.VendorBankAccounts.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(VendorBankAccount entity)
    {
        _context.VendorBankAccounts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.VendorBankAccounts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}