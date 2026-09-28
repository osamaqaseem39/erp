using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Api.Validators.Inventory;
using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/stock")]
[Authorize]
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetByItem(Guid itemId)
    {
        return await HandleAsync(() => _service.GetByItemAsync(itemId));
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
    {
        return await HandleAsync(() => _service.GetByWarehouseAsync(warehouseId));
    }

    [HttpGet("level")]
    public async Task<IActionResult> GetStockLevel(Guid itemId, Guid warehouseId)
    {
        return await HandleAsync(() => _service.GetStockLevelAsync(itemId, warehouseId));
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> GetStockLedger(Guid itemId, Guid warehouseId)
    {
        // Note: GetStockLedgerAsync not available in current interface
        return BadRequest(new { Error = "Stock ledger functionality not yet implemented" });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto dto)
    {
        var validationResult = await _adjustmentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.AdjustStockAsync(dto));
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock([FromBody] StockTransferDto dto)
    {
        var validationResult = await _transferValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        // Note: TransferStockAsync not available in current interface
        return BadRequest(new { Error = "Stock transfer functionality not yet implemented" });
    }
}