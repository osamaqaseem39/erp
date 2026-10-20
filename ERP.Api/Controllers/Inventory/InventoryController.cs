using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory")]
[Authorize]
[PermissionResource("inventory.stock")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class InventoryController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger) : base(logger)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("stock-balance")]
    [ProducesResponseType(typeof(StockBalanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetStockBalance([FromQuery] Guid itemId, [FromQuery] Guid? warehouseId = null)
    {
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            return BadRequest(new { error = "Warehouse ID is required." });

        var stockBalance = await _inventoryService.GetStockBalanceAsync(itemId, warehouseId.Value, GetTenantId());
        return stockBalance == null ? NotFound() : Ok(stockBalance);
    }

    [HttpGet("stock-balances")]
    [ProducesResponseType(typeof(IEnumerable<StockBalanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllStockBalances([FromQuery] Guid? warehouseId = null)
    {
        var stockBalances = await _inventoryService.GetAllStockBalancesAsync(GetTenantId(), GetCompanyId(), warehouseId);
        return Ok(stockBalances);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto)
    {
        var result = await _inventoryService.TransferStockAsync(dto, GetTenantId(), GetUserId().ToString());
        return result
            ? Ok(new SuccessResponse { Success = true })
            : BadRequest(new { error = "Could not create transfer." });
    }

    [HttpGet("transfer/{id:guid}")]
    [ProducesResponseType(typeof(StockTransferDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var transfer = await _inventoryService.GetStockTransferByIdAsync(id, GetTenantId());
        return transfer == null ? NotFound() : Ok(transfer);
    }

    [HttpPost("transfer/{id:guid}/complete")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CompleteTransfer(Guid id)
    {
        var userId = GetUserId().ToString();
        var result = await _inventoryService.UpdateTransferStatusAsync(id, GetTenantId(), "Completed", userId, userId);
        return result
            ? Ok(new SuccessResponse { Success = true })
            : BadRequest(new { error = "Could not complete transfer." });
    }

    [HttpGet("reports/valuation")]
    [ProducesResponseType(typeof(InventoryValuationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetInventoryValuation(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? asOfDate = null)
    {
        var valuation = await _inventoryService.GetInventoryValuationAsync(GetTenantId(), warehouseId, asOfDate);
        return Ok(valuation);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
    private Guid GetCompanyId() => User.RequireCompanyId();
}
