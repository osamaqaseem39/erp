using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/tax-codes")]
[Authorize]
[PermissionResource("finance.taxCodes")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class TaxCodesController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITaxCodeService _service;

    public TaxCodesController(ITaxCodeService service, ILogger<TaxCodesController> logger)
        : base(logger)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaxCodeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaxCodeDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] string? taxType = null, [FromQuery] bool activeOnly = false)
    {
        var tenantId = GetTenantId();
        IEnumerable<TaxCodeDto> result;
        if (!string.IsNullOrWhiteSpace(taxType))
            result = await _service.GetByTypeAsync(taxType, tenantId);
        else if (activeOnly)
            result = await _service.GetActiveAsync(tenantId);
        else
            result = await _service.GetAllAsync(tenantId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateTaxCodeDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to create tax code (code unique, rate >= 0, posting GL account required)"
            });
        return CreatedAtAction(nameof(GetById), new { id }, new { TaxCodeId = id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxCodeDto dto)
    {
        dto.Id = id;
        var ok = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to update tax code" });
        return Ok(new MessageResponse { Message = "Tax code updated successfully" });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        if (!ok) return NotFound();
        return Ok(new MessageResponse { Message = "Tax code deleted successfully" });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
