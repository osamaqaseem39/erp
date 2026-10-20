using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/cost-centers")]
[Authorize]
[PermissionResource("finance.costCenters")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CostCentersController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICostCenterService _service;

    public CostCentersController(ICostCenterService service, ILogger<CostCentersController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CostCenterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAllAsync(tenantId, isActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CostCenterDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCostCenterDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid or duplicate cost center." });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCostCenterDto dto)
    {
        dto.CostCenterId = id;
        var result = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        if (!ok)
        {
            var deactivated = await _service.SoftDeactivateAsync(id, GetTenantId());
            return deactivated ? Ok(new { deactivated = true }) : NotFound();
        }
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
