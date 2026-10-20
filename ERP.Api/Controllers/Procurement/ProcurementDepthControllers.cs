using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/landed-cost-vouchers")]
[Authorize]
[PermissionResource("procurement.landedCost")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class LandedCostVoucherController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILandedCostVoucherService _service;
    public LandedCostVoucherController(ILandedCostVoucherService service, ILogger<LandedCostVoucherController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLandedCostVoucherDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLandedCostVoucherDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(Guid id) =>
        await _service.PostAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/procurement/vendor-contracts")]
[Authorize]
[PermissionResource("procurement.vendorContracts")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class VendorContractController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IVendorContractService _service;
    public VendorContractController(IVendorContractService service, ILogger<VendorContractController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByVendor([FromQuery] Guid vendorId, [FromQuery] Guid? itemId = null)
    {
        var tenantId = HttpContext.RequireTenantId();
        if (itemId.HasValue)
        {
            var active = await _service.GetActiveAsync(vendorId, itemId, tenantId);
            return active == null ? NotFound() : Ok(active);
        }
        return Ok(await _service.GetByVendorAsync(vendorId, tenantId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorContractDto dto)
    {
        var created = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorContractDto dto)
    {
        dto.Id = id;
        var updated = await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}
