using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using ERP.Api.Errors;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Api.Validators;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
[ProducesBadRequest]
[ProducesUnauthorized]
public class AuthController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAuthApiService _authService;
    private readonly LoginValidator _loginValidator;
    private readonly RefreshTokenValidator _refreshTokenValidator;

    public AuthController(
        IAuthApiService authService,
        LoginValidator loginValidator,
        RefreshTokenValidator refreshTokenValidator,
        ILogger<AuthController> logger) : base(logger)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _authService.LoginAsync(loginDto));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(refreshTokenDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _authService.RefreshTokenAsync(refreshTokenDto));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(refreshTokenDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _authService.LogoutAsync(refreshTokenDto.RefreshToken));
    }

    /// <summary>The signed-in user, their roles and permission codes, and their organization.</summary>
    [HttpGet("me")]
    [Authorize]
    [SignedInOnly("Every signed-in user reads their own session.")]
    [ProducesResponseType(typeof(CurrentSessionDto), OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> Me() =>
        await HandleAsync(() => _authService.GetCurrentSessionAsync(User.RequireUserId(), HttpContext.RequireTenantId(), User.IsSuperAdmin()));

    /// <summary>Ends all of the caller's sessions on every device (this one included once its access token expires).</summary>
    [HttpPost("sign-out-everywhere")]
    [Authorize]
    [SignedInOnly("Everyone can end their own sessions.")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> SignOutEverywhere() =>
        await HandleAsync(() => _authService.SignOutEverywhereAsync(User.RequireUserId()));

    [HttpPost("change-password")]
    [SignedInOnly("Everyone can change their own password.")]
    [Authorize]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userId = GetCurrentUserId();
        return await HandleAsync(() => _authService.ChangePasswordAsync(
            userId,
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword));
    }

    /// <summary>Request a password-reset token. Always returns success (does not reveal whether the email exists).</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        return await HandleAsync(() => _authService.ForgotPasswordAsync(dto));
    }

    /// <summary>Complete a password reset with email + token + new password. Also accepts a request-only body (legacy).</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        return await HandleAsync(() => _authService.ResetPasswordAsync(resetPasswordDto));
    }

    /// <summary>Lists the caller's open sign-in sessions (devices).</summary>
    [HttpGet("sessions")]
    [Authorize]
    [SignedInOnly("Everyone can list their own sessions.")]
    [ProducesResponseType(typeof(IReadOnlyList<AuthSessionDto>), OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> ListSessions() =>
        await HandleAsync(() => _authService.ListSessionsAsync(User.RequireUserId(), User.TryGetSessionId()));

    /// <summary>Ends one of the caller's sessions (other device or this one).</summary>
    [HttpDelete("sessions/{sessionId:guid}")]
    [Authorize]
    [SignedInOnly("Everyone can revoke their own sessions.")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    [ProducesNotFound]
    [ProducesForbidden]
    public async Task<IActionResult> RevokeSession(Guid sessionId)
    {
        var ok = await _authService.RevokeSessionAsync(User.RequireUserId(), sessionId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        return await HandleAsync(() => _authService.VerifyEmailAsync(verifyEmailDto.Token));
    }

    /// <summary>Whether the signed-in user has TOTP 2FA enabled.</summary>
    [HttpGet("2fa/status")]
    [Authorize]
    [SignedInOnly("Everyone can read their own 2FA status.")]
    [ProducesResponseType(typeof(TwoFactorStatusDto), OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> TwoFactorStatus() =>
        await HandleAsync(() => _authService.GetTwoFactorStatusAsync(User.RequireUserId()));

    /// <summary>Begin TOTP setup: returns Base32 secret + otpauth URI. Confirm with POST 2fa/enable.</summary>
    [HttpPost("2fa/setup")]
    [Authorize]
    [SignedInOnly("Everyone can set up their own 2FA.")]
    [ProducesResponseType(typeof(TwoFactorSetupDto), OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> TwoFactorSetup() =>
        await HandleAsync(() => _authService.BeginTwoFactorSetupAsync(User.RequireUserId()));

    /// <summary>Confirm pending TOTP secret with a valid authenticator code.</summary>
    [HttpPost("2fa/enable")]
    [Authorize]
    [SignedInOnly("Everyone can enable their own 2FA.")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> TwoFactorEnable([FromBody] TwoFactorCodeDto dto) =>
        await HandleAsync(() => _authService.EnableTwoFactorAsync(User.RequireUserId(), dto.Code));

    /// <summary>Disable TOTP (requires password + current authenticator code).</summary>
    [HttpPost("2fa/disable")]
    [Authorize]
    [SignedInOnly("Everyone can disable their own 2FA.")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    [ProducesForbidden]
    public async Task<IActionResult> TwoFactorDisable([FromBody] TwoFactorDisableDto dto) =>
        await HandleAsync(() => _authService.DisableTwoFactorAsync(User.RequireUserId(), dto.Password, dto.Code));

    /// <summary>Complete login after password returned <c>requiresTwoFactor</c>.</summary>
    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> TwoFactorVerify([FromBody] TwoFactorVerifyLoginDto dto) =>
        await HandleAsync(() => _authService.VerifyTwoFactorLoginAsync(dto));

    private Guid GetCurrentUserId() => User.RequireUserId();
}
