using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using ERP.Api.Validators.Inventory;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/stock-transfers")]
public class StockTransferController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IStockTransferService _service;
    private readonly CreateStockTransferValidator _createValidator;
    private readonly UpdateStockTransferValidator _updateValidator;

    public StockTransferController(
        IStockTransferService service,
        CreateStockTransferValidator createValidator,
        UpdateStockTransferValidator updateValidator,
        ILogger<StockTransferController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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

    [HttpGet("from-warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByFromWarehouse(Guid warehouseId)
    {
        return await HandleAsync(() => _service.GetByFromWarehouseAsync(warehouseId));
    }

    [HttpGet("to-warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByToWarehouse(Guid warehouseId)
    {
        return await HandleAsync(() => _service.GetByToWarehouseAsync(warehouseId));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockTransferDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteAsync(id));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        return await HandleAsync(() => _service.ApproveAsync(id));
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        return await HandleAsync(() => _service.CompleteAsync(id));
    }
}