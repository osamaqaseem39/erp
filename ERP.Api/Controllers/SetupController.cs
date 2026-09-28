using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Core;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/setup")]
[EnableRateLimiting("auth")]
public class SetupController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISetupService _setupService;

    public SetupController(
        ISetupService setupService,
        ILogger<SetupController> logger) : base(logger)
    {
        _setupService = setupService;
    }

    [HttpGet("super-admin/exists")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckSuperAdminExists()
    {
        return await HandleAsync(async () =>
        {
            var exists = await _setupService.IsSuperAdminExistsAsync();
            return new { Exists = exists };
        });
    }

    [HttpPost("super-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> SetupSuperAdmin([FromBody] SuperAdminSetupDto dto)
    {
        return await HandleAsync(async () =>
        {
            if (await _setupService.IsSuperAdminExistsAsync())
                throw new ERP.Shared.Exceptions.ValidationException("Super admin already exists. Setup can only be performed once.");

            return await _setupService.SetupSuperAdminAsync(dto);
        });
    }
}
