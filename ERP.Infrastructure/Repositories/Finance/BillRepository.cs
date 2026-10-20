using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BillRepository : IBillRepository
{
    private readonly ERPDbContext _context;

    public BillRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Bill?> GetByIdAsync(Guid id)
    {
        return await _context.Bills
            .Include(b => b.BillLines)
            .FirstOrDefaultAsync(b => b.Id == id || b.BillId == id);
    }

    public async Task<IEnumerable<Bill>> GetAllAsync()
    {
        return await _context.Bills
            .Include(b => b.BillLines)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetBySupplierAsync(Guid supplierId)
    {
        return await _context.Bills
            .Include(b => b.BillLines)
            .Where(b => b.SupplierId == supplierId || b.VendorId == supplierId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetByStatusAsync(string status)
    {
        return await _context.Bills
            .Include(b => b.BillLines)
            .Where(b => b.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Bills
            .Include(b => b.BillLines)
            .Where(b => b.BillDate >= startDate && b.BillDate <= endDate)
            .ToListAsync();
    }

    public async Task<Bill?> GetByNumberAsync(string billNumber)
    {
        return await _context.Bills
            .Include(b => b.BillLines)
            .FirstOrDefaultAsync(b => b.BillNumber == billNumber);
    }

    public async Task<Guid> CreateAsync(Bill bill)
    {
        if (bill.Id == Guid.Empty) bill.Id = Guid.NewGuid();
        if (bill.BillId == Guid.Empty) bill.BillId = bill.Id;
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
        return bill.Id;
    }

    public async Task<bool> UpdateAsync(Bill bill)
    {
        _context.Bills.Update(bill);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bill = await GetByIdAsync(id);
        if (bill == null) return false;

        _context.Bills.Remove(bill);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
