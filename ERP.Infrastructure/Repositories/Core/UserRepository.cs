using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Extensions;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

/// <summary>
/// Users of the current tenant, through EF Core so the tenant guard and the audit trail apply.
/// (Replaces the Core.sp_* stored procedures, which didn't filter by the caller's tenant.)
/// First-run setup creates its user directly and doesn't use this repository.
/// </summary>
public class UserRepository : TenantScopedRepositoryBase, IUserRepository
{
    private static readonly SortMap<User> SortFields = new SortMap<User>("fullName")
        .Add("fullName", u => u.FullName)
        .Add("email", u => u.PrimaryEmail)
        .Add("lastSignIn", u => u.LastLoginDate)
        .Add("createdAt", u => u.CreatedAt);

    public UserRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider)
    {
    }

    private IQueryable<User> Users => Scoped(Context.Users);

    public Guid TenantId => CurrentTenantId;

    public async Task<User?> GetByIdAsync(Guid id) => await Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await Users.AsNoTracking().OrderBy(u => u.FullName).Take(PagedRequest.MaxPageSize).ToListAsync();

    public async Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize) =>
        await GetPagedAsync(new UserPagedQuery { PageNumber = pageNumber, PageSize = pageSize });

    public async Task<PagedResult<User>> GetPagedAsync(UserPagedQuery query, CancellationToken cancellationToken = default)
    {
        var users = Users.AsNoTracking();
        var now = DateTime.UtcNow;
        switch (query.Status?.ToLowerInvariant())
        {
            case "active": users = users.Where(u => u.IsActive); break;
            case "inactive": users = users.Where(u => !u.IsActive); break;
            case "locked": users = users.Where(u => u.IsLocked && u.LockedUntil > now); break;
        }
        if (query.RoleId.HasValue)
            users = users.Where(u => Context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == query.RoleId));
        if (query.WithoutRole == true)
            users = users.Where(u => !Context.UserRoles.Any(ur => ur.UserId == u.Id));
        var term = query.NormalizedSearchTerm;
        if (term != null)
            users = users.Where(u => (u.FullName != null && u.FullName.Contains(term)) || (u.PrimaryEmail != null && u.PrimaryEmail.Contains(term)));
        return await users.ToPagedResultAsync(query, SortFields, cancellationToken);
    }

    /// <summary>Within the current tenant only: the same address may exist in another organization.</summary>
    public async Task<User?> GetByEmailAsync(string email) =>
        await Users.FirstOrDefaultAsync(u => u.PrimaryEmail == email);

    public async Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId) =>
        tenantId == CurrentTenantId ? await GetByEmailAsync(email) : null;

    public async Task<User?> GetByUsernameAsync(string username) =>
        await Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<IEnumerable<User>> GetByCompanyAsync(Guid companyId) =>
        await Users.AsNoTracking().Where(u => u.CompanyId == companyId).ToListAsync();

    public async Task<IEnumerable<User>> GetByRoleAsync(Guid roleId) =>
        await Users.AsNoTracking().Where(u => Context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId)).ToListAsync();

    public async Task<Guid> CreateAsync(User user)
    {
        Context.Users.Add(Owned(user));
        await Context.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        if (user.TenantId != CurrentTenantId) return false;
        if (Context.Entry(user).State == EntityState.Detached) Context.Users.Update(user);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;
        Context.UserRoles.RemoveRange(Context.UserRoles.Where(ur => ur.UserId == id));
        Context.Users.Remove(user);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid id) => await Users.AnyAsync(u => u.Id == id);

    public async Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = expires;
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;
        user.LastLoginDate = DateTime.UtcNow;
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;
        user.PasswordHash = passwordHash;
        return await Context.SaveChangesAsync() > 0;
    }
}
