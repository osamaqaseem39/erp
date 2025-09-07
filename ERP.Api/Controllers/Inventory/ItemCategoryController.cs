using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/categories")]
[Authorize]
public class ItemCategoryController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IItemCategoryService _categoryService;
    private readonly ILogger<ItemCategoryController> _logger;

    public ItemCategoryController(IItemCategoryService categoryService, ILogger<ItemCategoryController> logger) : base(logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemCategoryDto dto)
    {
        try
        {
            var result = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { CategoryId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item category");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item category {CategoryId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? parentId = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            IEnumerable<ItemCategoryDto> categories;
            
            if (parentId.HasValue)
                categories = await _categoryService.GetByParentAsync(parentId.Value);
            else if (isActive.HasValue && isActive.Value)
                categories = await _categoryService.GetActiveAsync();
            else
                categories = await _categoryService.GetAllAsync();
                
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all item categories");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemCategoryDto dto)
    {
        try
        {
            dto.Id = id;
            var result = await _categoryService.UpdateAsync(dto);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item category {CategoryId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item category {CategoryId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree()
    {
        try
        {
            // Note: IItemCategoryService doesn't have GetCategoryTreeAsync method
            // This would need to be implemented or use a different approach
            var allCategories = await _categoryService.GetAllAsync();
            return Ok(allCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category tree");
            return BadRequest(new { Error = ex.Message });
        }
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