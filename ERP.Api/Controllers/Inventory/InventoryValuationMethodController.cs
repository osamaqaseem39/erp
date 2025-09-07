using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/valuation-methods")]
public class InventoryValuationMethodController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IInventoryValuationMethodService _service;
    private readonly ILogger<InventoryValuationMethodController> _logger;

    public InventoryValuationMethodController(IInventoryValuationMethodService service, ILogger<InventoryValuationMethodController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAllAsync(tenantId, isActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryValuationMethodDto dto)
    {
        var tenantId = GetTenantId();
        var createdBy = GetCurrentUser();
        var result = await _service.CreateAsync(dto, tenantId, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] InventoryValuationMethodDto dto)
    {
        dto.Id = id;
        var tenantId = GetTenantId();
        var updatedBy = GetCurrentUser();
        var result = await _service.UpdateAsync(dto, tenantId, updatedBy);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var success = await _service.DeleteAsync(id, tenantId);
        return success ? NoContent() : NotFound();
    }

    private Guid GetTenantId()
    {
        // Implementation to get tenant ID from context
        return Guid.NewGuid(); // Placeholder
    }

    private string GetCurrentUser()
    {
        // Implementation to get current user from context
        return "system"; // Placeholder
    }
}