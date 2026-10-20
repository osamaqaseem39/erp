using ERP.Api.Errors;
using ERP.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/quotes")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class QuoteController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IQuoteService _service;
    private readonly CreateQuoteValidator _createValidator;
    private readonly UpdateQuoteValidator _updateValidator;

    public QuoteController(
        IQuoteService service,
        CreateQuoteValidator createValidator,
        UpdateQuoteValidator updateValidator,
        ILogger<QuoteController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuoteDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuoteDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync(GetTenantId()));
    }

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<QuoteDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        return await HandleAsync(() => _service.GetByCustomerAsync(customerId, GetTenantId()));
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<QuoteDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status, GetTenantId()));
    }

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<QuoteDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate, GetTenantId()));
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateQuoteDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuoteDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));
    }

    [HttpPost("{id}/send")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SendQuote(Guid id)
    {
        var ok = await _service.SendQuoteAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to send quote (must be Draft)" });
        return Ok(new MessageResponse { Message = "Quote sent successfully" });
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _service.ApproveAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to approve quote (status, tenant, or creator cannot approve)" });
        return Ok(new MessageResponse { Message = "Quote approved successfully" });
    }

    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reject(Guid id)
    {
        var ok = await _service.RejectAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to reject quote" });
        return Ok(new MessageResponse { Message = "Quote rejected successfully" });
    }

    [HttpPost("{id}/convert")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ConvertToSalesOrder(Guid id)
    {
        var salesOrderId = await _service.ConvertToSalesOrderAsync(id, GetTenantId(), GetUserId());
        if (salesOrderId == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to convert quote (must be Accepted)" });
        return Ok(new { SalesOrderId = salesOrderId });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
