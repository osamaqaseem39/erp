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
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> VerifyEmailAsync(string token);
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

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto == null)
            throw new ValidationException("Reset password DTO cannot be null");

        if (!string.IsNullOrWhiteSpace(resetPasswordDto.Token) &&
            !string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword))
        {
            return await _authService.CompletePasswordResetAsync(
                resetPasswordDto.Email,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);
        }

        if (string.IsNullOrWhiteSpace(resetPasswordDto.Email) || resetPasswordDto.TenantId == Guid.Empty)
            throw new ValidationException("Email and TenantId are required to request a password reset");

        return await _authService.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.TenantId);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Token cannot be empty");

        return await _authService.VerifyEmailAsync(token);
    }
}
