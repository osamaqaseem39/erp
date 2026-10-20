using ERP.Shared.DTOs.Core;

namespace ERP.Application.Core;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(Guid userId);
    Task<bool> LogoutByRefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ForgotPasswordAsync(string email, Guid tenantId);
    /// <summary>Legacy alias — prefer <see cref="ForgotPasswordAsync"/>.</summary>
    Task<bool> ResetPasswordAsync(string email, Guid tenantId);
    Task<bool> CompletePasswordResetAsync(string email, string token, string newPassword);
    Task<bool> VerifyEmailAsync(string token);
    Task<CurrentSessionDto> GetCurrentSessionAsync(Guid userId, Guid tenantId, bool isSuperAdmin);
    Task<IReadOnlyList<AuthSessionDto>> ListSessionsAsync(Guid userId, Guid? currentSessionId = null);
    Task<bool> RevokeSessionAsync(Guid userId, Guid sessionId);

    Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId);
    Task<TwoFactorSetupDto> BeginTwoFactorSetupAsync(Guid userId);
    Task<bool> EnableTwoFactorAsync(Guid userId, string code);
    Task<bool> DisableTwoFactorAsync(Guid userId, string password, string code);
    Task<AuthResponseDto?> VerifyTwoFactorLoginAsync(string twoFactorToken, string code);
}
