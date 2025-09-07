using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Api.Validators;
using ERP.Shared.DTOs.Core;
using FluentValidation;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/auth")]
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
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _authService.LoginAsync(loginDto));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(refreshTokenDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _authService.RefreshTokenAsync(refreshTokenDto));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(refreshTokenDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _authService.LogoutAsync(refreshTokenDto.RefreshToken));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        return await HandleAsync(() => _authService.ChangePasswordAsync(changePasswordDto));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        return await HandleAsync(() => _authService.ResetPasswordAsync(resetPasswordDto));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        return await HandleAsync(() => _authService.VerifyEmailAsync(verifyEmailDto.Token));
    }
}