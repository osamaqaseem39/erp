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
[Route("api/sales/invoices")]
[Authorize]
[PermissionResource("sales.invoices")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SalesInvoiceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISalesInvoiceService _service;

    public SalesInvoiceController(ISalesInvoiceService service, ILogger<SalesInvoiceController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto)
    {
        var tenantId = GetTenantIdFromContext();
        var userId = GetUserIdFromContext();
        var result = await _service.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { InvoiceId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SalesInvoiceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantIdFromContext();
        var invoice = await _service.GetByIdAsync(id, tenantId);
        if (invoice == null)
            return NotFound();
        return Ok(invoice);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SalesInvoiceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] string? status = null)
    {
        var tenantId = GetTenantIdFromContext();
        IEnumerable<SalesInvoiceDto> invoices;

        if (customerId.HasValue)
            invoices = await _service.GetByCustomerAsync(customerId.Value, tenantId);
        else if (!string.IsNullOrEmpty(status))
            invoices = await _service.GetByStatusAsync(status, tenantId);
        else
            invoices = await _service.GetAllAsync(tenantId);

        return Ok(invoices);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesInvoiceDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });

        var tenantId = GetTenantIdFromContext();
        var userId = GetUserIdFromContext();
        var result = await _service.UpdateAsync(dto, tenantId, userId);
        if (!result)
            return NotFound();
        return Ok(new MessageResponse { Message = "Sales invoice updated successfully" });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantIdFromContext();
        var result = await _service.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return Ok(new MessageResponse { Message = "Sales invoice deleted successfully" });
    }

    [HttpPost("{id}/post")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PostInvoice(Guid id)
    {
        var tenantId = GetTenantIdFromContext();
        var userId = GetUserIdFromContext();
        var result = await _service.PostInvoiceAsync(id, tenantId, userId);
        if (!result)
            return NotFound();
        return Ok(new MessageResponse { Message = "Sales invoice posted successfully" });
    }

    [HttpPost("{id}/void")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> VoidInvoice(Guid id)
    {
        var tenantId = GetTenantIdFromContext();
        var userId = GetUserIdFromContext();
        var result = await _service.VoidInvoiceAsync(id, tenantId, userId);
        if (!result)
            return NotFound();
        return Ok(new MessageResponse { Message = "Sales invoice voided successfully" });
    }

    private Guid GetTenantIdFromContext() => HttpContext.RequireTenantId();

    private Guid GetUserIdFromContext() => User.RequireUserId();
}
