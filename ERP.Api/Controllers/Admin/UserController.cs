using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class UserController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<UserController> logger) : base(logger)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Ok)]
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
            return new IdResponse { Id = result };
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _userService.GetByIdAsync(id));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] string? role = null, [FromQuery] bool? isActive = null, [FromQuery] string? searchTerm = null)
    {
        return await HandleAsync(() => _userService.GetAllAsync());
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        return await HandleAsync(async () =>
        {
            dto.Id = id;
            var result = await _userService.UpdateAsync(dto);
            return new SuccessResponse { Success = result };
        });
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id)
    {
        return await HandleAsync(async () =>
        {
            var ok = await _userService.ActivateAsync(id);
            if (!ok) throw new ERP.Shared.Exceptions.NotFoundException("User not found");
            return new SuccessResponse { Success = true };
        });
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return await HandleAsync(async () =>
        {
            var ok = await _userService.DeactivateAsync(id);
            if (!ok) throw new ERP.Shared.Exceptions.NotFoundException("User not found");
            return new SuccessResponse { Success = true };
        });
    }

    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordDto dto)
    {
        return await HandleAsync(async () =>
        {
            dto.UserId = id;
            var result = await _userService.ResetPasswordAsync(dto);
            return new SuccessResponse { Success = result };
        });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(async () =>
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
                throw new ERP.Shared.Exceptions.NotFoundException("User not found");
            return new SuccessResponse { Success = true };
        });
    }
}
