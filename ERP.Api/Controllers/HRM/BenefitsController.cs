using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/benefits")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BenefitsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBenefitsService _benefitsService;

    public BenefitsController(IBenefitsService benefitsService, ILogger<BenefitsController> logger) : base(logger)
    {
        _benefitsService = benefitsService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BenefitDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateBenefitDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _benefitsService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BenefitDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var benefit = await _benefitsService.GetByIdAsync(id, tenantId);
        if (benefit == null)
            return NotFound();
        return Ok(benefit);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenefitDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null, [FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var benefits = await _benefitsService.GetAllAsync(tenantId, category, isActive);
        return Ok(benefits);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BenefitDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] BenefitDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _benefitsService.UpdateAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/enroll")]
    [ProducesResponseType(typeof(BenefitDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> EnrollEmployee(Guid id, [FromBody] EnrollEmployeeDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _benefitsService.EnrollEmployeeAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/unenroll")]
    [ProducesResponseType(typeof(BenefitDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UnenrollEmployee(Guid id, [FromBody] UnenrollEmployeeDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _benefitsService.UnenrollEmployeeAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _benefitsService.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
