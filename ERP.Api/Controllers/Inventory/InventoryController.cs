using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger) : base(logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpGet("stock-balance")]
    public async Task<IActionResult> GetStockBalance([FromQuery] Guid itemId, [FromQuery] Guid? warehouseId = null)
    {
        try
        {
            var tenantId = GetTenantId();
            if (!warehouseId.HasValue)
                return BadRequest(new { Error = "Warehouse ID is required" });
            
            var stockBalance = await _inventoryService.GetStockBalanceAsync(itemId, warehouseId.Value, tenantId);
            return Ok(stockBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock balance for item {ItemId}", itemId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("stock-balances")]
    public async Task<IActionResult> GetAllStockBalances([FromQuery] Guid? warehouseId = null, [FromQuery] Guid? itemId = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var companyId = GetCompanyId(); // Assuming this method exists or needs to be implemented
            var stockBalances = await _inventoryService.GetAllStockBalancesAsync(tenantId, companyId, warehouseId);
            return Ok(stockBalances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all stock balances");
            return BadRequest(new { Error = ex.Message });
        }
    }


    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _inventoryService.TransferStockAsync(dto, tenantId, userId.ToString());
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory transfer");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("transfer/{id}")]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var transfer = await _inventoryService.GetStockTransferByIdAsync(id, tenantId);
            if (transfer == null)
                return NotFound();
            return Ok(transfer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory transfer {TransferId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("transfer/{id}/complete")]
    public async Task<IActionResult> CompleteTransfer(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _inventoryService.UpdateTransferStatusAsync(id, tenantId, "Completed", userId.ToString(), userId.ToString());
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing inventory transfer {TransferId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("reports/valuation")]
    public async Task<IActionResult> GetInventoryValuation([FromQuery] Guid? warehouseId = null, [FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var tenantId = GetTenantId();
            // Note: IInventoryService doesn't have GetInventoryValuationAsync method
            // This would need to be implemented or use a different approach
            return Ok(new { Message = "Valuation functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory valuation");
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

    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("company_id")?.Value;
        if (Guid.TryParse(companyIdClaim, out var companyId))
            return companyId;
        throw new UnauthorizedAccessException("Company ID not found in token");
    }
}