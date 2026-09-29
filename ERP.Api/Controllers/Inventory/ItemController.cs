using ERP.Api.Security;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/items")]
[Authorize]
public class ItemController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService, ILogger<ItemController> logger) : base(logger)
    {
        _itemService = itemService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var item = await _itemService.GetByIdAsync(id, tenantId);
        
        if (item == null)
            return NotFound();
            
        return Ok(item);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll([FromQuery] Guid companyId, [FromQuery] Guid? categoryId = null)
    {
        var tenantId = GetTenantId();
        IEnumerable<ItemDto> items;

        if (categoryId.HasValue)
            items = await _itemService.GetByCategoryAsync(categoryId.Value, tenantId);
        else
            items = await _itemService.GetAllAsync(tenantId, companyId);

        return Ok(items);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetLowStockItems([FromQuery] Guid companyId)
    {
        var tenantId = GetTenantId();
        var items = await _itemService.GetLowStockItemsAsync(tenantId, companyId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemDto dto)
    {
        var tenantId = GetTenantId();
        var createdBy = GetCurrentUser();
        var item = await _itemService.CreateAsync(dto, tenantId, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> Update(Guid id, [FromBody] UpdateItemDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var updatedBy = GetCurrentUser();
        var item = await _itemService.UpdateAsync(dto, tenantId, updatedBy);
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _itemService.DeleteAsync(id, tenantId);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private string GetCurrentUser() => User.RequireUserId().ToString();
}