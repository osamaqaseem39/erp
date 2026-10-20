using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

[ApiController]
[Route("api/core/branches")]
[Authorize]
[PermissionResource("core.branches")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BranchesController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICoreMasterService _service;

    public BranchesController(ICoreMasterService service, ILogger<BranchesController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BranchDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null) =>
        Ok(await _service.ListBranchesAsync(HttpContext.RequireTenantId(), isActive));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BranchDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetBranchAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BranchDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
    {
        var row = await _service.CreateBranchAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id = row.Id }, row);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BranchDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchDto dto) =>
        Ok(await _service.UpdateBranchAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteBranchAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return NoContent();
    }
}
