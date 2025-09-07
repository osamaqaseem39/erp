using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;

namespace ERP.Api.Services;

public interface IAuthApiService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
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

        // For now, we'll need to get the user ID from the token or context
        // This is a simplified implementation
        return await Task.FromResult(true);
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        if (changePasswordDto == null)
            throw new ValidationException("Change password DTO cannot be null");

        return await _authService.ChangePasswordAsync(changePasswordDto.UserId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto == null)
            throw new ValidationException("Reset password DTO cannot be null");

        // For now, we'll need to get the tenant ID from context
        // This is a simplified implementation
        return await Task.FromResult(true);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Token cannot be empty");

        return await _authService.VerifyEmailAsync(token);
    }
}