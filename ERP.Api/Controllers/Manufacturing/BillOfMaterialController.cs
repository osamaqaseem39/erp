using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/boms")]
[Authorize]
[PermissionResource("manufacturing.boms")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BillOfMaterialController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBillOfMaterialService _service;

    public BillOfMaterialController(IBillOfMaterialService service, ILogger<BillOfMaterialController> logger)
        : base(logger) => _service = service;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BillOfMaterialDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, GetTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BillOfMaterialDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? finishedGoodItemId = null)
        => Ok(await _service.GetAllAsync(GetTenantId(), finishedGoodItemId));

    [HttpPost]
    [ProducesResponseType(typeof(BillOfMaterialDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateBillOfMaterialDto dto)
    {
        var created = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BillOfMaterialDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBillOfMaterialDto dto)
    {
        dto.Id = id;
        return Ok(await _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(BillOfMaterialDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id)
        => Ok(await _service.ActivateAsync(id, GetTenantId(), GetUserId()));

    [HttpPost("{id:guid}/obsolete")]
    [ProducesResponseType(typeof(BillOfMaterialDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Obsolete(Guid id)
        => Ok(await _service.ObsoleteAsync(id, GetTenantId(), GetUserId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
