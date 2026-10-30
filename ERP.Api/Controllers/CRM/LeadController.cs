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
[Route("api/crm/leads")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class LeadController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeadService _service;
    private readonly CreateLeadValidator _createValidator;
    private readonly UpdateLeadValidator _updateValidator;

    public LeadController(
        ILeadService service,
        CreateLeadValidator createValidator,
        UpdateLeadValidator updateValidator,
        ILogger<LeadController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeadDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeadDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpGet("source/{source}")]
    [ProducesResponseType(typeof(IEnumerable<LeadDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetBySource(string source) =>
        await HandleAsync(() => _service.GetBySourceAsync(source, GetTenantId()));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<LeadDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status) =>
        await HandleAsync(() => _service.GetByStatusAsync(status, GetTenantId()));

    [HttpGet("assigned/{assignedTo}")]
    [ProducesResponseType(typeof(IEnumerable<LeadDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByAssignedTo(Guid assignedTo) =>
        await HandleAsync(() => _service.GetByAssignedToAsync(assignedTo, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));

    /// <summary>Leads, customers and contacts sharing this email or phone.</summary>
    [HttpGet("duplicates")]
    [ProducesResponseType(typeof(IReadOnlyList<LeadDuplicateDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Duplicates([FromQuery] string? email, [FromQuery] string? phone, [FromQuery] Guid? excludeId) =>
        Ok(await _service.FindDuplicatesAsync(email, phone, excludeId, GetTenantId()));

    [HttpPost("{id}/qualify")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Qualify(Guid id)
    {
        var ok = await _service.QualifyAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to qualify lead (must be New)" });
        return Ok(new MessageResponse { Message = "Lead qualified" });
    }

    [HttpPost("{id}/lose")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkLost(Guid id, [FromBody] LoseLeadDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LostReason))
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "A lost reason is required" });
        var ok = await _service.MarkLostAsync(id, dto, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to mark lead lost (must be New or Qualified)" });
        return Ok(new MessageResponse { Message = "Lead marked as lost" });
    }

    [HttpPost("{id}/convert")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ConvertToCustomer(Guid id)
    {
        var customerId = await _service.ConvertToCustomerAsync(id, GetTenantId(), GetUserId());
        if (customerId == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to convert lead (must be Qualified)" });
        return Ok(new { CustomerId = customerId });
    }

    [HttpPost("{id}/convert-to-opportunity")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ConvertToOpportunity(Guid id, [FromBody] ConvertLeadToOpportunityDto? dto)
    {
        var opportunityId = await _service.ConvertToOpportunityAsync(id, GetTenantId(), GetUserId(), dto);
        if (opportunityId == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to convert lead (must be Qualified)" });
        return Ok(new { OpportunityId = opportunityId });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
