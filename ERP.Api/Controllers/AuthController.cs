using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using ERP.Api.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Api.Validators;
using ERP.Shared.DTOs.Core;
using System.Security.Claims;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
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
    public async Task<IActionResult> Me() =>
        await HandleAsync(() => _authService.GetCurrentSessionAsync(User.RequireUserId(), HttpContext.RequireTenantId(), User.IsSuperAdmin()));

    /// <summary>Ends all of the caller's sessions on every device (this one included once its access token expires).</summary>
    [HttpPost("sign-out-everywhere")]
    [Authorize]
    [SignedInOnly("Everyone can end their own sessions.")]
    public async Task<IActionResult> SignOutEverywhere() =>
        await HandleAsync(() => _authService.SignOutEverywhereAsync(User.RequireUserId()));

    [HttpPost("change-password")]
    [SignedInOnly("Everyone can change their own password.")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userId = GetCurrentUserId();
        return await HandleAsync(() => _authService.ChangePasswordAsync(
            userId,
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        return await HandleAsync(() => _authService.ResetPasswordAsync(resetPasswordDto));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        return await HandleAsync(() => _authService.VerifyEmailAsync(verifyEmailDto.Token));
    }

    private Guid GetCurrentUserId() => User.RequireUserId();
}
