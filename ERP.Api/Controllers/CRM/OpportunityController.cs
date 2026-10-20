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
[Route("api/crm/opportunities")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class OpportunityController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IOpportunityService _service;
    private readonly CreateOpportunityValidator _createValidator;
    private readonly UpdateOpportunityValidator _updateValidator;

    public OpportunityController(
        IOpportunityService service,
        CreateOpportunityValidator createValidator,
        UpdateOpportunityValidator updateValidator,
        ILogger<OpportunityController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OpportunityDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync());

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByCustomer(Guid customerId) =>
        await HandleAsync(() => _service.GetByCustomerAsync(customerId));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status) =>
        await HandleAsync(() => _service.GetByStatusAsync(status));

    [HttpGet("assigned/{assignedTo}")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByAssignedTo(Guid assignedTo) =>
        await HandleAsync(() => _service.GetByAssignedToAsync(assignedTo));

    [HttpGet("stage/{stage}")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStage(string stage) =>
        await HandleAsync(() => _service.GetByStageAsync(stage));

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id));

    [HttpPost("{id}/win")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkWon(Guid id)
    {
        var ok = await _service.MarkWonAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to mark opportunity won (must be Open)" });
        return Ok(new MessageResponse { Message = "Opportunity marked as won" });
    }

    [HttpPost("{id}/lose")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkLost(Guid id, [FromBody] CloseOpportunityRequest? body = null)
    {
        var ok = await _service.MarkLostAsync(id, GetTenantId(), GetUserId(), body?.Reason);
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to mark opportunity lost (must be Open)" });
        return Ok(new MessageResponse { Message = "Opportunity marked as lost" });
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to cancel opportunity (must be Open)" });
        return Ok(new MessageResponse { Message = "Opportunity cancelled" });
    }

    [HttpPost("{id}/convert")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ConvertToQuote(Guid id)
    {
        var quoteId = await _service.ConvertToQuoteAsync(id, GetTenantId(), GetUserId());
        if (quoteId == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to convert opportunity (must be Won)" });
        return Ok(new { QuoteId = quoteId });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

public sealed class CloseOpportunityRequest
{
    public string? Reason { get; set; }
}
