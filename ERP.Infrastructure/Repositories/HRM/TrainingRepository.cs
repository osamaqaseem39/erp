using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class TrainingRepository : ITrainingRepository
{
    private readonly ERPDbContext _context;

    public TrainingRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Training?> GetByIdAsync(Guid id)
    {
        return await _context.Trainings
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Training>> GetAllAsync()
    {
        return await _context.Trainings
            .Include(t => t.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.Trainings
            .Include(t => t.Employee)
            .Where(t => t.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Training>> GetByStatusAsync(string status)
    {
        return await _context.Trainings
            .Include(t => t.Employee)
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Training>> GetByTypeAsync(string trainingType)
    {
        return await _context.Trainings
            .Include(t => t.Employee)
            .Where(t => t.TrainingType == trainingType)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Training training)
    {
        _context.Trainings.Add(training);
        await _context.SaveChangesAsync();
        return training.Id;
    }

    public async Task<bool> UpdateAsync(Training training)
    {
        _context.Trainings.Update(training);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var training = await _context.Trainings.FindAsync(id);
        if (training == null) return false;

        _context.Trainings.Remove(training);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}