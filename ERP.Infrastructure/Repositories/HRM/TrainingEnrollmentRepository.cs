using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class TrainingEnrollmentRepository : ITrainingEnrollmentRepository
{
    private readonly ERPDbContext _context;

    public TrainingEnrollmentRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<TrainingEnrollment?> GetByIdAsync(int id)
    {
        return await _context.TrainingEnrollments.FindAsync(id);
    }

    public async Task<IEnumerable<TrainingEnrollment>> GetAllAsync()
    {
        return await _context.TrainingEnrollments.ToListAsync();
    }

    public async Task<TrainingEnrollment> AddAsync(TrainingEnrollment entity)
    {
        _context.TrainingEnrollments.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TrainingEnrollment entity)
    {
        _context.TrainingEnrollments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.TrainingEnrollments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}