using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UserController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<UserController> logger) : base(logger)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        return await HandleAsync(async () =>
        {
            if (dto.TenantId == Guid.Empty && HttpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is Guid headerTenantId)
            {
                dto.TenantId = headerTenantId;
            }

            if (dto.TenantId == Guid.Empty)
                throw new ERP.Shared.Exceptions.ValidationException("Tenant ID is required.");

            var result = await _userService.CreateAsync(dto);
            return new { UserId = result };
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _userService.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? role = null, [FromQuery] bool? isActive = null, [FromQuery] string? searchTerm = null)
    {
        return await HandleAsync(() => _userService.GetAllAsync());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        return await HandleAsync(async () =>
        {
            dto.Id = id;
            var result = await _userService.UpdateAsync(dto);
            return new { Success = result };
        });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        return BadRequest(new { message = "Activate functionality not yet implemented" });
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return BadRequest(new { message = "Deactivate functionality not yet implemented" });
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordDto dto)
    {
        return await HandleAsync(async () =>
        {
            dto.UserId = id;
            var result = await _userService.ResetPasswordAsync(dto);
            return new { Success = result };
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(async () =>
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
                throw new ERP.Shared.Exceptions.NotFoundException("User not found");
            return new { Success = true };
        });
    }
}
