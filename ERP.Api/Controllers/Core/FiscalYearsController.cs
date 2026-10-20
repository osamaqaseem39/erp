using ERP.Api.Abstractions;
using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Core;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

[ApiController]
[Route("api/core/fiscal-years")]
[Authorize]
[PermissionResource("core.fiscalYears")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class FiscalYearsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IFiscalYearService _service;

    public FiscalYearsController(IFiscalYearService service, ILogger<FiscalYearsController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FiscalYearDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var result = await _service.GetAllAsync(GetTenantId(), status);
        return Ok(result);
    }

    [HttpGet("covering")]
    [ProducesResponseType(typeof(FiscalYearDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCovering([FromQuery] DateTime date)
    {
        var result = await _service.GetActiveCoveringAsync(GetTenantId(), date);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FiscalYearDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateFiscalYearDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Invalid fiscal year (overlap, dates, or name)." });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFiscalYearDto dto)
    {
        dto.FiscalYearId = id;
        var result = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        if (result == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to update fiscal year." });
        return Ok(new { id = result });
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Close(Guid id)
    {
        var ok = await _service.CloseAsync(id, GetTenantId(), GetUserId());
        if (!ok) return NotFound();
        return Ok(new MessageResponse { Message = "Fiscal year closed." });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to delete (not found or referenced by journals)." });
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
