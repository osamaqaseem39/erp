using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/categories")]
[Authorize]
[PermissionResource("inventory.categories")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ItemCategoryController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IItemCategoryService _categoryService;

    public ItemCategoryController(IItemCategoryService categoryService, ILogger<ItemCategoryController> logger) : base(logger)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateItemCategoryDto dto)
    {
        var id = await _categoryService.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid or duplicate category." });
        return CreatedAtAction(nameof(GetById), new { id }, new { CategoryId = id });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ItemCategoryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id, GetTenantId());
        return category == null ? NotFound() : Ok(category);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemCategoryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? parentId = null, [FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        IEnumerable<ItemCategoryDto> categories =
            parentId.HasValue ? await _categoryService.GetByParentAsync(parentId, tenantId)
            : isActive == true ? await _categoryService.GetActiveAsync(tenantId)
            : await _categoryService.GetAllAsync(tenantId);
        return Ok(categories);
    }

    [HttpGet("tree")]
    [ProducesResponseType(typeof(IEnumerable<ItemCategoryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCategoryTree()
        => Ok(await _categoryService.GetAllAsync(GetTenantId()));

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemCategoryDto dto)
    {
        dto.Id = id;
        var ok = await _categoryService.UpdateAsync(dto, GetTenantId(), GetUserId());
        return ok ? Ok(new { Success = true }) : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _categoryService.DeleteAsync(id, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
