using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class ShiftRepository : IShiftRepository
{
    private readonly ERPDbContext _context;

    public ShiftRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Shift?> GetByIdAsync(Guid id)
    {
        return await _context.Shifts.FindAsync(id);
    }

    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        return await _context.Shifts.ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetActiveAsync()
    {
        return await _context.Shifts
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Shift shift)
    {
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();
        return shift.Id;
    }

    public async Task<bool> UpdateAsync(Shift shift)
    {
        _context.Shifts.Update(shift);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var shift = await _context.Shifts.FindAsync(id);
        if (shift == null) return false;

        _context.Shifts.Remove(shift);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}