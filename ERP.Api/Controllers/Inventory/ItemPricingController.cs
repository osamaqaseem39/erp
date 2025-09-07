using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/item-pricing")]
public class ItemPricingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IItemPricingService _service;
    private readonly ILogger<ItemPricingController> _logger;

    public ItemPricingController(IItemPricingService service, ILogger<ItemPricingController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetByItem(Guid itemId, [FromQuery] string? priceListName = null)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByItemAsync(itemId, tenantId, priceListName);
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
    public async Task<IActionResult> Create([FromBody] ItemPricingDto dto)
    {
        var tenantId = GetTenantId();
        var createdBy = GetCurrentUser();
        var result = await _service.CreateAsync(dto, tenantId, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ItemPricingDto dto)
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