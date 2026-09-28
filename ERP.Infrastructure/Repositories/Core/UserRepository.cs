using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces;
using ERP.Shared.DTOs;

namespace ERP.Infrastructure.Repositories.Core;

public class UserRepository : IUserRepository
{
    private readonly IStoredProcedureExecutor _executor;

    public UserRepository(IStoredProcedureExecutor executor)
    {
        _executor = executor;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _executor.ExecuteSingleAsync<User>("[Core].[sp_GetUserById]", new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _executor.ExecuteQueryAsync<User>("[Core].[sp_GetAllUsers]");
    }

    public async Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var users = await _executor.ExecuteQueryAsync<User>("getAllUsersPaged", new { PageNumber = pageNumber, PageSize = pageSize });
        var totalCount = await _executor.ExecuteScalarAsync<int>("getUsersCount");
        
        return new PagedResult<User>
        {
            Items = users.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _executor.ExecuteSingleAsync<User>("[Core].[sp_GetUserByEmail]", new { PrimaryEmail = email });
    }

    public async Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId)
    {
        return await _executor.ExecuteSingleAsync<User>("[Core].[sp_GetUserByEmailAndTenant]", new { PrimaryEmail = email, TenantId = tenantId });
    }

    public async Task<IEnumerable<User>> GetByCompanyAsync(Guid companyId)
    {
        return await _executor.ExecuteQueryAsync<User>("getUsersByCompany", new { CompanyId = companyId });
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(Guid roleId)
    {
        return await _executor.ExecuteQueryAsync<User>("getUsersByRole", new { RoleId = roleId });
    }

    public async Task<Guid> CreateAsync(User user)
    {
        var parameters = new
        {
            Id = user.Id,
            TenantId = user.TenantId,
            CompanyId = user.CompanyId,
            Salutation = user.Salutation,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            JobTitle = user.JobTitle,
            Department = user.Department,
            Nationality = user.Nationality,
            PrimaryEmail = user.PrimaryEmail,
            SecondaryEmail = user.SecondaryEmail,
            PrimaryPhone = user.PrimaryPhone,
            SecondaryPhone = user.SecondaryPhone,
            MobilePhone = user.MobilePhone,
            Fax = user.Fax,
            LinkedIn = user.LinkedIn,
            Twitter = user.Twitter,
            Facebook = user.Facebook,
            Instagram = user.Instagram,
            PreferredCommunicationMethod = user.PreferredCommunicationMethod,
            LanguagePreference = user.LanguagePreference,
            TimeZone = user.TimeZone,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            Status = user.Status,
            LastLoginDate = user.LastLoginDate,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockedUntil = user.LockedUntil,
            EmailVerificationToken = user.EmailVerificationToken,
            PasswordResetToken = user.PasswordResetToken,
            PasswordResetExpires = user.PasswordResetExpires,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpires = user.RefreshTokenExpires,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            IsTenantOwner = user.IsTenantOwner
        };

        return await _executor.ExecuteScalarAsync<Guid>("[Core].[sp_CreateUser]", parameters);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var parameters = new
        {
            Id = user.Id,
            TenantId = user.TenantId,
            CompanyId = user.CompanyId,
            Salutation = user.Salutation,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            JobTitle = user.JobTitle,
            Department = user.Department,
            Nationality = user.Nationality,
            PrimaryEmail = user.PrimaryEmail,
            SecondaryEmail = user.SecondaryEmail,
            PrimaryPhone = user.PrimaryPhone,
            SecondaryPhone = user.SecondaryPhone,
            MobilePhone = user.MobilePhone,
            Fax = user.Fax,
            LinkedIn = user.LinkedIn,
            Twitter = user.Twitter,
            Facebook = user.Facebook,
            Instagram = user.Instagram,
            PreferredCommunicationMethod = user.PreferredCommunicationMethod,
            LanguagePreference = user.LanguagePreference,
            TimeZone = user.TimeZone,
            Status = user.Status,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockedUntil = user.LockedUntil,
            EmailVerificationToken = user.EmailVerificationToken,
            PasswordResetToken = user.PasswordResetToken,
            PasswordResetExpires = user.PasswordResetExpires,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,
            IsTenantOwner = user.IsTenantOwner
        };

        var rowsAffected = await _executor.ExecuteNonQueryAsync("[Core].[sp_UpdateUser]", parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Note: This method needs TenantId, but it's not in the signature
        // This is a design issue - DeleteAsync should accept TenantId
        // For now, we'll need to get the user first to get TenantId
        var user = await GetByIdAsync(id);
        if (user == null) return false;
        
        var rowsAffected = await _executor.ExecuteNonQueryAsync("[Core].[sp_DeleteUser]", new { Id = id, TenantId = user.TenantId });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var exists = await _executor.ExecuteScalarAsync<bool>("userExists", new { Id = id });
        return exists;
    }

    public async Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires)
    {
        var rowsAffected = await _executor.ExecuteNonQueryAsync("updateUserRefreshToken", 
            new { UserId = userId, RefreshToken = refreshToken, RefreshTokenExpires = expires });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        var rowsAffected = await _executor.ExecuteNonQueryAsync("updateUserLastLogin", 
            new { UserId = userId, LastLoginAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash)
    {
        // Note: This method needs TenantId, but it's not in the signature
        // This is a design issue - UpdatePasswordAsync should accept TenantId
        // For now, we'll need to get the user first to get TenantId
        var user = await GetByIdAsync(userId);
        if (user == null) return false;
        
        var rowsAffected = await _executor.ExecuteNonQueryAsync("[Core].[sp_UpdateUserPassword]", 
            new { Id = userId, TenantId = user.TenantId, PasswordHash = passwordHash });
        return rowsAffected > 0;
    }
}