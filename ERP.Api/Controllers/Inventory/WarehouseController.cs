using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/warehouses")]
[Authorize]
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
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        var result = await _warehouseService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { WarehouseId = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound();
        return Ok(warehouse);
    }

    [HttpGet]
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        dto.Id = id;
        var result = await _warehouseService.UpdateAsync(dto);
        return Ok(new { Success = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _warehouseService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _warehouseService.ActivateAsync(id);
        return Ok(new { Success = result });
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _warehouseService.DeactivateAsync(id);
        return Ok(new { Success = result });
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}