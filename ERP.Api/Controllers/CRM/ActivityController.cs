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
[Route("api/crm/activities")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ActivityController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IActivityService _service;
    private readonly CreateActivityValidator _createValidator;
    private readonly UpdateActivityValidator _updateValidator;

    public ActivityController(
        IActivityService service,
        CreateActivityValidator createValidator,
        UpdateActivityValidator updateValidator,
        ILogger<ActivityController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ActivityDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync());

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByCustomer(Guid customerId) =>
        await HandleAsync(() => _service.GetByCustomerAsync(customerId));

    [HttpGet("lead/{leadId}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByLead(Guid leadId) =>
        await HandleAsync(() => _service.GetByLeadAsync(leadId));

    [HttpGet("opportunity/{opportunityId}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByOpportunity(Guid opportunityId) =>
        await HandleAsync(() => _service.GetByOpportunityAsync(opportunityId));

    [HttpGet("related/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByRelatedEntity(string entityType, Guid entityId) =>
        await HandleAsync(() => _service.GetByEntityAsync(entityType, entityId));

    [HttpGet("assigned/{assignedTo}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByAssignedTo(Guid assignedTo) =>
        await HandleAsync(() => _service.GetByAssignedToAsync(assignedTo));

    [HttpGet("type/{activityType}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByType(string activityType) =>
        await HandleAsync(() => _service.GetByTypeAsync(activityType));

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate) =>
        await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate));

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateActivityDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityDto dto)
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

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ok = await _service.CompleteAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to complete activity (must be Open)" });
        return Ok(new MessageResponse { Message = "Activity completed" });
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to cancel activity (must be Open)" });
        return Ok(new MessageResponse { Message = "Activity cancelled" });
    }

    [HttpPost("{id}/reschedule")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleActivityRequest request)
    {
        var ok = await _service.RescheduleAsync(id, request.NewDate, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to reschedule activity (must be Open)" });
        return Ok(new MessageResponse { Message = "Activity rescheduled" });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

public class RescheduleActivityRequest
{
    public DateTime NewDate { get; set; }
}
