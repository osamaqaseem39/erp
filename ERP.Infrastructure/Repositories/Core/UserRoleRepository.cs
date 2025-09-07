using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly ERPDbContext _context;

    public UserRoleRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<UserRole?> GetByIdAsync(int id)
    {
        return await _context.UserRoles.FindAsync(id);
    }

    public async Task<IEnumerable<UserRole>> GetAllAsync()
    {
        return await _context.UserRoles.ToListAsync();
    }

    public async Task<UserRole> AddAsync(UserRole entity)
    {
        _context.UserRoles.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserRole entity)
    {
        _context.UserRoles.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.UserRoles.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}