using System.Security.Claims;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;

namespace ERP.Api.Services;

public interface IAuthApiService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> VerifyEmailAsync(string token);
    Task<CurrentSessionDto> GetCurrentSessionAsync(Guid userId, Guid tenantId, bool isSuperAdmin);
    Task<bool> SignOutEverywhereAsync(Guid userId);
    Task<IReadOnlyList<AuthSessionDto>> ListSessionsAsync(Guid userId, Guid? currentSessionId);
    Task<bool> RevokeSessionAsync(Guid userId, Guid sessionId);
    Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId);
    Task<TwoFactorSetupDto> BeginTwoFactorSetupAsync(Guid userId);
    Task<bool> EnableTwoFactorAsync(Guid userId, string code);
    Task<bool> DisableTwoFactorAsync(Guid userId, string password, string code);
    Task<AuthResponseDto?> VerifyTwoFactorLoginAsync(TwoFactorVerifyLoginDto dto);
}

public class AuthApiService : IAuthApiService
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthApiService> _logger;

    public AuthApiService(IAuthService authService, ILogger<AuthApiService> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        if (loginDto == null)
            throw new ValidationException("Login DTO cannot be null");

        return await _authService.LoginAsync(loginDto);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        if (refreshTokenDto == null)
            throw new ValidationException("Refresh token DTO cannot be null");

        return await _authService.RefreshTokenAsync(refreshTokenDto);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ValidationException("Refresh token cannot be empty");

        return await _authService.LogoutByRefreshTokenAsync(refreshToken);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("User is required");
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            throw new ValidationException("Current and new passwords are required");

        return await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        if (dto == null)
            throw new ValidationException("Forgot password DTO cannot be null");
        if (string.IsNullOrWhiteSpace(dto.Email) || dto.TenantId == Guid.Empty)
            throw new ValidationException("Email and TenantId are required to request a password reset");

        return await _authService.ForgotPasswordAsync(dto.Email.Trim(), dto.TenantId);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto == null)
            throw new ValidationException("Reset password DTO cannot be null");

        if (!string.IsNullOrWhiteSpace(resetPasswordDto.Token) &&
            !string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword))
        {
            if (!string.IsNullOrWhiteSpace(resetPasswordDto.ConfirmPassword)
                && !string.Equals(resetPasswordDto.NewPassword, resetPasswordDto.ConfirmPassword, StringComparison.Ordinal))
                throw new ValidationException("New password and confirmation do not match");

            return await _authService.CompletePasswordResetAsync(
                resetPasswordDto.Email,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);
        }

        if (string.IsNullOrWhiteSpace(resetPasswordDto.Email) || resetPasswordDto.TenantId == Guid.Empty)
            throw new ValidationException("Email and TenantId are required to request a password reset");

        return await _authService.ForgotPasswordAsync(resetPasswordDto.Email, resetPasswordDto.TenantId);
    }

    public Task<CurrentSessionDto> GetCurrentSessionAsync(Guid userId, Guid tenantId, bool isSuperAdmin) =>
        _authService.GetCurrentSessionAsync(userId, tenantId, isSuperAdmin);

    public Task<bool> SignOutEverywhereAsync(Guid userId) => _authService.LogoutAsync(userId);

    public Task<IReadOnlyList<AuthSessionDto>> ListSessionsAsync(Guid userId, Guid? currentSessionId) =>
        _authService.ListSessionsAsync(userId, currentSessionId);

    public Task<bool> RevokeSessionAsync(Guid userId, Guid sessionId) =>
        _authService.RevokeSessionAsync(userId, sessionId);

    public async Task<bool> VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Token cannot be empty");

        return await _authService.VerifyEmailAsync(token);
    }

    public Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId) =>
        _authService.GetTwoFactorStatusAsync(userId);

    public Task<TwoFactorSetupDto> BeginTwoFactorSetupAsync(Guid userId) =>
        _authService.BeginTwoFactorSetupAsync(userId);

    public async Task<bool> EnableTwoFactorAsync(Guid userId, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Authenticator code is required");
        return await _authService.EnableTwoFactorAsync(userId, code.Trim());
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId, string password, string code)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Password and authenticator code are required");
        return await _authService.DisableTwoFactorAsync(userId, password, code.Trim());
    }

    public async Task<AuthResponseDto?> VerifyTwoFactorLoginAsync(TwoFactorVerifyLoginDto dto)
    {
        if (dto == null)
            throw new ValidationException("2FA verify DTO cannot be null");
        if (string.IsNullOrWhiteSpace(dto.TwoFactorToken) || string.IsNullOrWhiteSpace(dto.Code))
            throw new ValidationException("Two-factor token and code are required");
        return await _authService.VerifyTwoFactorLoginAsync(dto.TwoFactorToken.Trim(), dto.Code.Trim());
    }
}
