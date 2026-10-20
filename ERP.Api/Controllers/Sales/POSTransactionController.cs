using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.Abstractions;
using ERP.Api.OpenApi;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/pos-transactions")]
[Authorize]
[PermissionResource("sales.posTransactions")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class POSTransactionController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPOSTransactionService _service;

    public POSTransactionController(IPOSTransactionService service, ILogger<POSTransactionController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreatePOSTransactionDto dto)
    {
        var result = await _service.CreateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        return CreatedAtAction(nameof(GetById), new { id = result }, new { TransactionId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(POSTransactionDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _service.GetByIdAsync(id, GetTenantIdFromContext());
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<POSTransactionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? sessionId = null, [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var tenantId = GetTenantIdFromContext();
        IEnumerable<POSTransactionDto> transactions;
        if (sessionId.HasValue)
            transactions = await _service.GetBySessionAsync(sessionId.Value, tenantId);
        else if (customerId.HasValue)
            transactions = await _service.GetByCustomerAsync(customerId.Value, tenantId);
        else if (startDate.HasValue && endDate.HasValue)
            transactions = await _service.GetByDateRangeAsync(startDate.Value, endDate.Value, tenantId);
        else
            transactions = await _service.GetAllAsync(tenantId);
        return Ok(transactions);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePOSTransactionDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        var result = await _service.UpdateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!result) return NotFound();
        return Ok(new MessageResponse { Message = "POS transaction updated successfully" });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, GetTenantIdFromContext());
        if (!result) return NotFound();
        return Ok(new MessageResponse { Message = "POS transaction deleted successfully" });
    }

    private Guid GetTenantIdFromContext() => HttpContext.RequireTenantId();
    private Guid GetUserIdFromContext() => User.RequireUserId();
}
