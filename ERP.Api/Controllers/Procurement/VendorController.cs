using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/vendors")]
[Authorize]
public class VendorController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IVendorService _vendorService;
    private readonly ILogger<VendorController> _logger;

    public VendorController(IVendorService vendorService, ILogger<VendorController> logger) : base(logger)
    {
        _vendorService = vendorService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _vendorService.AddVendorAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { VendorId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vendor");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var vendor = await _vendorService.GetVendorByIdAsync(id, tenantId);
            if (vendor == null)
                return NotFound();
            return Ok(vendor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor {VendorId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] string? category = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var query = new VendorQuery
            {
                TenantId = tenantId,
                SearchTerm = searchTerm,
                IsActive = isActive
            };
            var (vendors, totalCount) = await _vendorService.SearchVendorsAsync(query);
            return Ok(new { Vendors = vendors, TotalCount = totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all vendors");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorDto dto)
    {
        try
        {
            dto.VendorId = id;
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _vendorService.UpdateVendorAsync(dto, tenantId, userId);
            return Ok(new { Success = result != Guid.Empty });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vendor {VendorId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            // Note: IVendorService doesn't have ApproveAsync method
            // This would need to be implemented or use a different approach
            return BadRequest(new { Error = "Approve functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving vendor {VendorId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendVendorDto dto)
    {
        try
        {
            // Note: IVendorService doesn't have SuspendAsync method
            // This would need to be implemented or use a different approach
            return BadRequest(new { Error = "Suspend functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending vendor {VendorId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _vendorService.DeleteVendorAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vendor {VendorId}", id);
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