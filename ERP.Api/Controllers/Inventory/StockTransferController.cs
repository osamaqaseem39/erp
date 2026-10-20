using ERP.Api.Errors;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Inventory;
using ERP.Api.Validators.Inventory;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/stock-transfers")]
[Authorize]
[PermissionResource("inventory.stockTransfers")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
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
    [ProducesResponseType(typeof(StockTransferDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StockTransferDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(GetTenantId()));

    [HttpGet("from-warehouse/{warehouseId}")]
    [ProducesResponseType(typeof(IEnumerable<StockTransferDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByFromWarehouse(Guid warehouseId) =>
        Ok(await _service.GetByFromWarehouseAsync(warehouseId, GetTenantId()));

    [HttpGet("to-warehouse/{warehouseId}")]
    [ProducesResponseType(typeof(IEnumerable<StockTransferDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByToWarehouse(Guid warehouseId) =>
        Ok(await _service.GetByToWarehouseAsync(warehouseId, GetTenantId()));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<StockTransferDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status) =>
        Ok(await _service.GetByStatusAsync(status, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "Invalid transfer (warehouses, lines, quantities)." });
        return CreatedAtAction(nameof(GetById), new { id }, new { TransferId = id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockTransferDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var ok = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Update rejected (must be Draft)." });
        return Ok(new MessageResponse { Message = "Stock transfer updated successfully" });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _service.ApproveAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to approve (must be Draft; creator cannot approve; needs lines)." });
        return Ok(new MessageResponse { Message = "Stock transfer approved (InTransit)" });
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ok = await _service.CompleteAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to complete (must be InTransit; check stock)." });
        return Ok(new MessageResponse { Message = "Stock transfer completed" });
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to cancel transfer." });
        return Ok(new MessageResponse { Message = "Stock transfer cancelled" });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
