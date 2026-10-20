using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/warehouses")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class WarehouseController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger) : base(logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        dto.TenantId = GetTenantId();
        dto.CreatedBy = GetUserId().ToString();
        var result = await _warehouseService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { WarehouseId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WarehouseDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound();
        return Ok(warehouse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId = null, [FromQuery] bool? isActive = null)
    {
        IEnumerable<WarehouseDto> warehouses;
        
        if (isActive.HasValue && isActive.Value)
            warehouses = await _warehouseService.GetActiveAsync();
        else
            warehouses = await _warehouseService.GetAllAsync();
            
        return Ok(warehouses);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        dto.Id = id;
        var result = await _warehouseService.UpdateAsync(dto);
        return Ok(new { Success = result });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _warehouseService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _warehouseService.ActivateAsync(id);
        return Ok(new { Success = result });
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _warehouseService.DeactivateAsync(id);
        return Ok(new { Success = result });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}