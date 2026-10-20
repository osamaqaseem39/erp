using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/emails")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class AdminEmailController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAdminEmailService _service;

    public AdminEmailController(IAdminEmailService service, ILogger<AdminEmailController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission("admin.emails.view")]
    [ProducesResponseType(typeof(IEnumerable<AdminEmailDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        return Ok(await _service.GetAllAsync(tenantId, isActive));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("admin.emails.view")]
    [ProducesResponseType(typeof(AdminEmailDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetByIdAsync(id, GetTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [RequirePermission("admin.emails.create")]
    [ProducesResponseType(typeof(AdminEmailDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateAdminEmailDto dto)
    {
        var created = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("admin.emails.update")]
    [ProducesResponseType(typeof(AdminEmailDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdminEmailDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("admin.emails.delete")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, GetTenantId(), GetUserId());
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
