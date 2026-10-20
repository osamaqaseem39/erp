using ERP.Api.Security.Permissions;
using ERP.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces;
using ERP.Api.Validators.Inventory;
using ERP.Shared.DTOs.Inventory;
using ERP.Api.Security;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/stock")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class StockController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IStockService _service;
    private readonly StockAdjustmentValidator _adjustmentValidator;
    private readonly StockTransferValidator _transferValidator;

    public StockController(
        IStockService service,
        StockAdjustmentValidator adjustmentValidator,
        StockTransferValidator transferValidator,
        ILogger<StockController> logger) : base(logger)
    {
        _service = service;
        _adjustmentValidator = adjustmentValidator;
        _transferValidator = transferValidator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockBalanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleAsync(() => _service.GetByIdAsync(id));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StockBalanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
        => await HandleAsync(() => _service.GetAllAsync());

    [HttpGet("item/{itemId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<StockBalanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByItem(Guid itemId)
        => await HandleAsync(() => _service.GetByItemAsync(itemId));

    [HttpGet("warehouse/{warehouseId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<StockBalanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
        => await HandleAsync(() => _service.GetByWarehouseAsync(warehouseId));

    [HttpGet("level")]
    [ProducesResponseType(typeof(decimal), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetStockLevel([FromQuery] Guid itemId, [FromQuery] Guid warehouseId)
        => await HandleAsync(() => _service.GetStockLevelAsync(itemId, warehouseId));

    [HttpGet("ledger")]
    [ProducesResponseType(typeof(IEnumerable<StockLedgerDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetStockLedger(
        [FromQuery] Guid itemId,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = HttpContext.RequireTenantId();
        return Ok(await _service.GetStockLedgerAsync(itemId, tenantId, warehouseId, toDate));
    }

    [HttpPost("average-rate")]
    [ProducesResponseType(typeof(StockAverageRateDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CalculateAverageRate([FromBody] CalculateStockAverageRateRequest request)
    {
        if (request.ItemId == Guid.Empty)
            return BadRequest(new { Error = "ItemId is required." });
        var result = await _service.CalculateAverageRateAsync(HttpContext.RequireTenantId(), request);
        return Ok(result);
    }

    [HttpGet("average-rate")]
    [ProducesResponseType(typeof(StockAverageRateDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAverageRate(
        [FromQuery] Guid itemId,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? asOfDate = null,
        [FromQuery] bool updateItemCostPrice = false)
    {
        if (itemId == Guid.Empty)
            return BadRequest(new { Error = "itemId is required." });
        var result = await _service.CalculateAverageRateAsync(
            HttpContext.RequireTenantId(),
            new CalculateStockAverageRateRequest
            {
                ItemId = itemId,
                WarehouseId = warehouseId,
                AsOfDate = asOfDate,
                UpdateItemCostPrice = updateItemCostPrice
            });
        return Ok(result);
    }

    [HttpPost("adjust")]
    [RequirePermission("inventory.stock.adjust")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto dto)
    {
        var validationResult = await _adjustmentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.AdjustStockAsync(dto));
    }

    [HttpPost("transfer")]
    [RequirePermission("inventory.stockTransfers.create")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> TransferStock([FromBody] StockTransferDto dto)
    {
        var validationResult = await _transferValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var ok = await _service.TransferStockAsync(dto);
        return ok ? Ok(new { Success = true }) : BadRequest(new { Error = "Transfer failed (insufficient stock)." });
    }
}
