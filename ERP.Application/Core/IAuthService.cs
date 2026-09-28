using ERP.Shared.DTOs.Core;

namespace ERP.Application.Core;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(Guid userId);
    Task<bool> LogoutByRefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string email, Guid tenantId);
    Task<bool> CompletePasswordResetAsync(string email, string token, string newPassword);
    Task<bool> VerifyEmailAsync(string token);
}
