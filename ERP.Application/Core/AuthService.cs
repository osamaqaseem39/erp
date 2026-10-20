using System.Security.Cryptography;
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
    private readonly IPasswordResetNotifier _resetNotifier;
    private readonly IEffectivePermissions? _permissions;
    private readonly ISystemLogWriter? _systemLog;

    public AuthService(ERPDbContext db, ISessionStore sessions, ISecurityPolicyProvider policies,
        IPasswordResetNotifier? resetNotifier = null,
        IEffectivePermissions? permissions = null, ISystemLogWriter? systemLog = null)
    {
        _db = db;
        _sessions = sessions;
        _policies = policies;
        _resetNotifier = resetNotifier ?? new NullPasswordResetNotifier();
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
        // Tokens are now kept per session (RefreshTokens); clear the old single-token columns.
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;

        if (PasswordHelper.NeedsRehash(user.PasswordSalt))
        {
            var (hash, salt) = PasswordHelper.HashPassword(loginDto.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        if (user.TwoFactorEnabled && !string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            var challenge = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            user.TwoFactorChallengeTokenHash = SessionStore.Hash(challenge);
            user.TwoFactorChallengeExpires = DateTime.UtcNow.AddMinutes(5);
            await _db.SaveChangesAsync();
            await LogSecurityAsync("Information", "Sign-in pending 2FA", user.TenantId, user.Id, loginDto.PrimaryEmail);
            return new AuthResponseDto
            {
                RequiresTwoFactor = true,
                TwoFactorToken = challenge,
                User = MapToUserDto(user)
            };
        }

        user.LastLoginDate = DateTime.UtcNow;
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

    public Task<bool> ResetPasswordAsync(string email, Guid tenantId) => ForgotPasswordAsync(email, tenantId);

    public async Task<bool> ForgotPasswordAsync(string email, Guid tenantId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PrimaryEmail == email && u.TenantId == tenantId);
        // Always succeed so callers cannot probe which emails exist.
        if (user == null || !user.IsActive) return true;

        var raw = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var expires = DateTime.UtcNow.AddHours(1);
        user.PasswordResetToken = SessionStore.Hash(raw);
        user.PasswordResetExpires = expires;
        await _db.SaveChangesAsync();
        await _resetNotifier.NotifyAsync(user.PrimaryEmail ?? email, tenantId, raw, expires);
        await LogSecurityAsync("Information", "Password reset requested", user.TenantId, user.Id, user.PrimaryEmail);
        return true;
    }

    public async Task<bool> CompletePasswordResetAsync(string email, string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedException("Invalid or expired reset token");

        var tokenHash = SessionStore.Hash(token);
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.PrimaryEmail == email &&
            u.PasswordResetToken != null &&
            (u.PasswordResetToken == tokenHash || u.PasswordResetToken == token) &&
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
        await LogSecurityAsync("Information", "Password reset completed", user.TenantId, user.Id, user.PrimaryEmail);
        return true;
    }

    public async Task<IReadOnlyList<AuthSessionDto>> ListSessionsAsync(Guid userId, Guid? currentSessionId = null)
    {
        var rows = await _sessions.ListActiveAsync(userId);
        return rows.Select(s => new AuthSessionDto
        {
            SessionId = s.SessionId,
            CreatedAt = s.CreatedAt,
            IdleExpiresAt = s.IdleExpiresAt,
            SessionExpiresAt = s.SessionExpiresAt,
            IpAddress = s.IpAddress,
            UserAgent = s.UserAgent,
            IsCurrent = currentSessionId.HasValue && s.SessionId == currentSessionId.Value
        }).ToList();
    }

    public Task<bool> RevokeSessionAsync(Guid userId, Guid sessionId)
        => _sessions.EndSessionAsync(userId, sessionId, SessionStore.ReasonRevokedByUser);

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        if (user == null) return false;

        user.EmailVerificationToken = null;
        user.IsEmailVerified = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedException("Your account is not available. Please sign in again.");
        return new TwoFactorStatusDto { Enabled = user.TwoFactorEnabled };
    }

    public async Task<TwoFactorSetupDto> BeginTwoFactorSetupAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedException("Your account is not available. Please sign in again.");
        if (user.TwoFactorEnabled)
            throw new InvalidOperationException("Two-factor authentication is already enabled.");

        var secret = TotpHelper.GenerateSecret();
        user.TwoFactorPendingSecret = secret;
        await _db.SaveChangesAsync();

        var account = user.PrimaryEmail ?? user.Email ?? user.Username;
        return new TwoFactorSetupDto
        {
            Secret = secret,
            OtpAuthUri = TotpHelper.BuildOtpAuthUri("ERP", account, secret)
        };
    }

    public async Task<bool> EnableTwoFactorAsync(Guid userId, string code)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedException("Your account is not available. Please sign in again.");
        if (string.IsNullOrEmpty(user.TwoFactorPendingSecret))
            throw new InvalidOperationException("Start 2FA setup before enabling.");
        if (!TotpHelper.Verify(user.TwoFactorPendingSecret, code))
            throw new UnauthorizedException("Invalid authenticator code");

        user.TwoFactorSecret = user.TwoFactorPendingSecret;
        user.TwoFactorPendingSecret = null;
        user.TwoFactorEnabled = true;
        await _db.SaveChangesAsync();
        await LogSecurityAsync("Information", "2FA enabled", user.TenantId, user.Id, user.PrimaryEmail);
        return true;
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId, string password, string code)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedException("Your account is not available. Please sign in again.");
        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new InvalidOperationException("Two-factor authentication is not enabled.");
        if (!PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedException("Invalid credentials");
        if (!TotpHelper.Verify(user.TwoFactorSecret, code))
            throw new UnauthorizedException("Invalid authenticator code");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorPendingSecret = null;
        user.TwoFactorChallengeTokenHash = null;
        user.TwoFactorChallengeExpires = null;
        await _db.SaveChangesAsync();
        await LogSecurityAsync("Information", "2FA disabled", user.TenantId, user.Id, user.PrimaryEmail);
        return true;
    }

    public async Task<AuthResponseDto?> VerifyTwoFactorLoginAsync(string twoFactorToken, string code)
    {
        if (string.IsNullOrWhiteSpace(twoFactorToken) || string.IsNullOrWhiteSpace(code))
            throw new UnauthorizedException("Invalid credentials");

        var hash = SessionStore.Hash(twoFactorToken);
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.TwoFactorChallengeTokenHash == hash
            && u.TwoFactorChallengeExpires != null
            && u.TwoFactorChallengeExpires > DateTime.UtcNow);

        if (user == null || !user.IsActive || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new UnauthorizedException("Invalid or expired two-factor challenge");

        if (!TotpHelper.Verify(user.TwoFactorSecret, code))
        {
            await LogSecurityAsync("Warning", "Failed 2FA verify", user.TenantId, user.Id, user.PrimaryEmail);
            throw new UnauthorizedException("Invalid authenticator code");
        }

        user.TwoFactorChallengeTokenHash = null;
        user.TwoFactorChallengeExpires = null;
        user.LastLoginDate = DateTime.UtcNow;
        var policy = await _policies.GetAsync(user.TenantId);
        var issued = await _sessions.StartAsync(user, policy);
        await LogSecurityAsync("Information", "Signed in (2FA)", user.TenantId, user.Id, user.PrimaryEmail);
        return await ResponseAsync(user, issued.Token, issued.Row.SessionId, policy);
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
            TwoFactorEnabled = user.TwoFactorEnabled,
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
