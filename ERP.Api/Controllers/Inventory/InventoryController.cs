using ERP.Api.Security.Permissions;
using ERP.Api.Security;
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
    [RequirePermission("inventory.stock.view")]
    public async Task<IActionResult> GetStockBalance([FromQuery] Guid itemId, [FromQuery] Guid? warehouseId = null)
    {
        var tenantId = GetTenantId();
        if (!warehouseId.HasValue)
            return BadRequest(new { Error = "Warehouse ID is required" });
        
        var stockBalance = await _inventoryService.GetStockBalanceAsync(itemId, warehouseId.Value, tenantId);
        return Ok(stockBalance);
    }

    [HttpGet("stock-balances")]
    [RequirePermission("inventory.stock.view")]
    public async Task<IActionResult> GetAllStockBalances([FromQuery] Guid? warehouseId = null, [FromQuery] Guid? itemId = null)
    {
        var tenantId = GetTenantId();
        var companyId = GetCompanyId(); // Assuming this method exists or needs to be implemented
        var stockBalances = await _inventoryService.GetAllStockBalancesAsync(tenantId, companyId, warehouseId);
        return Ok(stockBalances);
    }


    [HttpPost("transfer")]
    [RequirePermission("inventory.stockTransfers.create")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _inventoryService.TransferStockAsync(dto, tenantId, userId.ToString());
        return Ok(new { Success = result });
    }

    [HttpGet("transfer/{id}")]
    [RequirePermission("inventory.stockTransfers.view")]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var tenantId = GetTenantId();
        var transfer = await _inventoryService.GetStockTransferByIdAsync(id, tenantId);
        if (transfer == null)
            return NotFound();
        return Ok(transfer);
    }

    [HttpPost("transfer/{id}/complete")]
    [RequirePermission("inventory.stockTransfers.complete")]
    public async Task<IActionResult> CompleteTransfer(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _inventoryService.UpdateTransferStatusAsync(id, tenantId, "Completed", userId.ToString(), userId.ToString());
        return Ok(new { Success = result });
    }

    [HttpGet("reports/valuation")]
    public async Task<IActionResult> GetInventoryValuation([FromQuery] Guid? warehouseId = null, [FromQuery] DateTime? asOfDate = null)
    {
        var tenantId = GetTenantId();
        // Note: IInventoryService doesn't have GetInventoryValuationAsync method
        // This would need to be implemented or use a different approach
        return Ok(new { Message = "Valuation functionality not yet implemented" });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();

    private Guid GetCompanyId() => User.RequireCompanyId();
}