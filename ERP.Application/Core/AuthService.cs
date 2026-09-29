using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Logging;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Core;

public class AuthService : IAuthService
{
    private readonly ERPDbContext _db;
    private readonly ISystemLogWriter? _systemLog;

    public AuthService(ERPDbContext db, ISystemLogWriter? systemLog = null)
    {
        _db = db;
        _systemLog = systemLog;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PrimaryEmail == loginDto.PrimaryEmail && u.TenantId == loginDto.TenantId);

        if (user == null || !user.IsActive)
        {
            // Only for real tenants, so made-up tenant ids can't fill the log with junk rows.
            if (await _db.Tenants.AnyAsync(t => t.Id == loginDto.TenantId))
                await LogSecurityAsync("Warning", "Failed sign-in: unknown or inactive account", loginDto.TenantId, user?.Id, loginDto.PrimaryEmail);
            throw new UnauthorizedException("Invalid credentials");
        }

        if (user.IsLocked && user.LockedUntil > DateTime.UtcNow)
        {
            await LogSecurityAsync("Warning", "Sign-in refused: account is locked", user.TenantId, user.Id, loginDto.PrimaryEmail);
            throw new UnauthorizedException("Account is locked");
        }

        if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
            }
            await _db.SaveChangesAsync();
            await LogSecurityAsync("Warning",
                user.IsLocked ? "Account locked after repeated failed sign-ins" : "Failed sign-in: wrong password",
                user.TenantId, user.Id, loginDto.PrimaryEmail);
            throw new UnauthorizedException("Invalid credentials");
        }

        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockedUntil = null;
        user.LastLoginDate = DateTime.UtcNow;

        if (PasswordHelper.NeedsRehash(user.PasswordSalt))
        {
            var (hash, salt) = PasswordHelper.HashPassword(loginDto.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        var refreshToken = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(JwtHelper.RefreshTokenExpiryDays);

        await _db.SaveChangesAsync();

        var roles = await GetUserRoleNamesAsync(user.Id);
        var token = JwtHelper.GenerateJwtToken(user.Id, user.PrimaryEmail ?? user.Email, roles, user.TenantId, user.CompanyId);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(JwtHelper.ExpiryMinutes),
            User = MapToUserDto(user, roles)
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);

        if (user == null ||
            user.RefreshTokenExpires == null ||
            user.RefreshTokenExpires < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        var newRefreshToken = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(JwtHelper.RefreshTokenExpiryDays);
        await _db.SaveChangesAsync();

        var roles = await GetUserRoleNamesAsync(user.Id);
        var token = JwtHelper.GenerateJwtToken(user.Id, user.PrimaryEmail ?? user.Email, roles, user.TenantId, user.CompanyId);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(JwtHelper.ExpiryMinutes),
            User = MapToUserDto(user, roles)
        };
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpires = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LogoutByRefreshTokenAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null) return true;

        user.RefreshToken = null;
        user.RefreshTokenExpires = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (!PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedException("Current password is incorrect");

        var (hash, salt) = PasswordHelper.HashPassword(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, Guid tenantId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PrimaryEmail == email && u.TenantId == tenantId);
        if (user == null) return true;

        user.PasswordResetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompletePasswordResetAsync(string email, string token, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.PrimaryEmail == email &&
            u.PasswordResetToken == token &&
            u.PasswordResetExpires != null &&
            u.PasswordResetExpires > DateTime.UtcNow);

        if (user == null)
            throw new UnauthorizedException("Invalid or expired reset token");

        var (hash, salt) = PasswordHelper.HashPassword(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.PasswordResetToken = null;
        user.PasswordResetExpires = null;
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        if (user == null) return false;

        user.EmailVerificationToken = null;
        user.IsEmailVerified = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<List<string>> GetUserRoleNamesAsync(Guid userId)
    {
        var roles = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.RoleName
        ).Where(n => n != null && n != "").Distinct().ToListAsync();

        return roles.Count > 0 ? roles! : new List<string> { "User" };
    }

    private static UserDto MapToUserDto(User user, List<string>? roles = null)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            CompanyId = user.CompanyId ?? Guid.Empty,
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
            Email = user.PrimaryEmail,
            SecondaryEmail = user.SecondaryEmail,
            PrimaryPhone = user.PrimaryPhone,
            SecondaryPhone = user.SecondaryPhone,
            MobilePhone = user.MobilePhone,
            Status = user.Status,
            LastLoginDate = user.LastLoginDate,
            IsLocked = user.IsLocked,
            IsActive = user.IsActive,
            RoleNames = roles ?? new List<string>(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }

    private Task LogSecurityAsync(string level, string message, Guid tenantId, Guid? userId, string? email) =>
        _systemLog?.WriteAsync(level, SystemLogSources.Security, message, tenantId, userId, method: "Login", properties: new { email }) ?? Task.CompletedTask;
}
