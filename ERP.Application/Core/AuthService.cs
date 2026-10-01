using ERP.Application.Core.Auth;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Logging;
using ERP.Infrastructure.Security;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Core;

public class AuthService : IAuthService
{
    private static readonly string[] SuperAdminRoles = ["SuperAdmin", "SUPER_ADMIN"];

    private readonly ERPDbContext _db;
    private readonly ISessionStore _sessions;
    private readonly ISecurityPolicyProvider _policies;
    private readonly IEffectivePermissions? _permissions;
    private readonly ISystemLogWriter? _systemLog;

    public AuthService(ERPDbContext db, ISessionStore sessions, ISecurityPolicyProvider policies,
        IEffectivePermissions? permissions = null, ISystemLogWriter? systemLog = null)
    {
        _db = db;
        _sessions = sessions;
        _policies = policies;
        _permissions = permissions;
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

        var policy = await _policies.GetAsync(user.TenantId);

        if (user.IsLocked && user.LockedUntil > DateTime.UtcNow)
        {
            await LogSecurityAsync("Warning", "Sign-in refused: account is locked", user.TenantId, user.Id, loginDto.PrimaryEmail);
            throw new UnauthorizedException("Account is locked");
        }
        if (user.IsLocked)
        {
            // The lock has run out: start counting again.
            user.IsLocked = false;
            user.LockedUntil = null;
            user.FailedLoginAttempts = 0;
        }

        if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= policy.LockoutThreshold)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(policy.LockoutMinutes);
            }
            await _db.SaveChangesAsync();
            await LogSecurityAsync("Warning",
                user.IsLocked ? "Account locked after repeated failed sign-ins" : "Failed sign-in: wrong password",
                user.TenantId, user.Id, loginDto.PrimaryEmail);
            throw new UnauthorizedException("Invalid credentials");
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginDate = DateTime.UtcNow;
        // Tokens are now kept per session (RefreshTokens); clear the old single-token columns.
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;

        if (PasswordHelper.NeedsRehash(user.PasswordSalt))
        {
            var (hash, salt) = PasswordHelper.HashPassword(loginDto.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        var issued = await _sessions.StartAsync(user, policy);
        await LogSecurityAsync("Information", "Signed in", user.TenantId, user.Id, loginDto.PrimaryEmail);
        return await ResponseAsync(user, issued.Token, issued.Row.SessionId, policy);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var rotated = await _sessions.RotateAsync(refreshTokenDto.RefreshToken);
        return await ResponseAsync(rotated.User, rotated.Token, rotated.Row.SessionId, rotated.Policy);
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        await _sessions.EndAllAsync(userId, SessionStore.ReasonSignOutEverywhere);
        return true;
    }

    public async Task<bool> LogoutByRefreshTokenAsync(string refreshToken)
    {
        await _sessions.EndAsync(refreshToken, SessionStore.ReasonSignOut);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (!PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            throw ValidationException.ForField("currentPassword", "Current password is incorrect.");
        (await _policies.GetAsync(user.TenantId)).EnsurePasswordAllowed(newPassword);

        var (hash, salt) = PasswordHelper.HashPassword(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        await _db.SaveChangesAsync();
        // Every session must sign in again with the new password.
        await _sessions.EndAllAsync(user.Id, SessionStore.ReasonPasswordChanged);
        await LogSecurityAsync("Information", "Password changed", user.TenantId, user.Id, user.PrimaryEmail);
        return true;
    }

    public async Task<CurrentSessionDto> GetCurrentSessionAsync(Guid userId, Guid tenantId, bool isSuperAdmin)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new UnauthorizedException("Your account is not available. Please sign in again.");
        var roles = await GetUserRoleNamesAsync(user.Id);
        var tenantName = await _db.Tenants.AsNoTracking().Where(t => t.Id == tenantId).Select(t => t.TenantName).FirstOrDefaultAsync();
        return new CurrentSessionDto
        {
            User = MapToUserDto(user, roles),
            Roles = roles,
            Permissions = await PermissionsAsync(user, isSuperAdmin),
            IsSuperAdmin = isSuperAdmin,
            TenantId = tenantId,
            TenantName = tenantName ?? string.Empty,
        };
    }

    private async Task<AuthResponseDto> ResponseAsync(User user, string refreshToken, Guid sessionId, SecurityPolicy policy)
    {
        var roles = await GetUserRoleNamesAsync(user.Id);
        var isSuperAdmin = roles.Any(r => SuperAdminRoles.Contains(r));
        var lifetime = JwtHelper.AccessTokenLifetime(TimeSpan.FromMinutes(policy.SessionTimeoutMinutes));
        var token = JwtHelper.GenerateJwtToken(user.Id, user.PrimaryEmail ?? user.Email, roles, user.TenantId, user.CompanyId, lifetime, sessionId);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            User = MapToUserDto(user, roles),
            Permissions = await PermissionsAsync(user, isSuperAdmin),
            IsSuperAdmin = isSuperAdmin,
        };
    }

    /// <summary>Empty for SuperAdmin, who passes every check.</summary>
    private async Task<List<string>> PermissionsAsync(User user, bool isSuperAdmin)
    {
        if (isSuperAdmin || _permissions == null) return new List<string>();
        return (await _permissions.GetAsync(user.TenantId, user.Id)).Order(StringComparer.Ordinal).ToList();
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
        (await _policies.GetAsync(user.TenantId)).EnsurePasswordAllowed(newPassword);

        var (hash, salt) = PasswordHelper.HashPassword(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.PasswordResetToken = null;
        user.PasswordResetExpires = null;
        await _db.SaveChangesAsync();
        await _sessions.EndAllAsync(user.Id, SessionStore.ReasonPasswordChanged);
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
