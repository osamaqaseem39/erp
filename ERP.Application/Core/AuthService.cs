using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;

namespace ERP.Application.Core;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;

    public AuthService(IUserRepository userRepository, ITenantRepository tenantRepository)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAndTenantAsync(loginDto.PrimaryEmail, loginDto.TenantId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedException("Invalid credentials");

        if (user.IsLocked && user.LockedUntil > DateTime.UtcNow)
            throw new UnauthorizedException("Account is locked");

        if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
            }
            await _userRepository.UpdateAsync(user);
            throw new UnauthorizedException("Invalid credentials");
        }

        // Reset failed login attempts on successful login
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockedUntil = null;
        user.LastLoginDate = DateTime.UtcNow;

        var refreshToken = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);

        await _userRepository.UpdateAsync(user);

        var token = JwtHelper.GenerateJwtToken(user.Id, user.PrimaryEmail, "User", user.TenantId);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(refreshTokenDto.RefreshToken));
        if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken || user.RefreshTokenExpires < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid refresh token");

        var newRefreshToken = JwtHelper.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);

        await _userRepository.UpdateAsync(user);

        var token = JwtHelper.GenerateJwtToken(user.Id, user.PrimaryEmail, "User", user.TenantId);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = MapToUserDto(user)
        };
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpires = null;

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect");

        var newPasswordHash = PasswordHelper.HashPassword(newPassword);
        return await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
    }

    public async Task<bool> ResetPasswordAsync(string email, Guid tenantId)
    {
        var user = await _userRepository.GetByEmailAndTenantAsync(email, tenantId);
        if (user == null) return false;

        // Generate reset token and send email (implementation would go here)
        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(token));
        if (user == null || user.EmailVerificationToken != token) return false;

        user.EmailVerificationToken = null;

        return await _userRepository.UpdateAsync(user);
    }

    private static UserDto MapToUserDto(User user)
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
            LastLoginDate = user.LastLoginDate,
            IsLocked = user.IsLocked,
            CompanyName = user.Company?.CompanyName ?? "",
            RoleNames = user.UserRoles?.Select(ur => ur.Role?.RoleName ?? "").ToList() ?? new List<string>(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }
}