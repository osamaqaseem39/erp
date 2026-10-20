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
[Route("api/crm/campaigns")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CampaignController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICampaignService _service;
    private readonly CreateCampaignValidator _createValidator;
    private readonly UpdateCampaignValidator _updateValidator;

    public CampaignController(
        ICampaignService service,
        CreateCampaignValidator createValidator,
        UpdateCampaignValidator updateValidator,
        ILogger<CampaignController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CampaignDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<CampaignDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status) =>
        await HandleAsync(() => _service.GetByStatusAsync(status, GetTenantId()));

    [HttpGet("type/{campaignType}")]
    [ProducesResponseType(typeof(IEnumerable<CampaignDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByType(string campaignType) =>
        await HandleAsync(() => _service.GetByTypeAsync(campaignType, GetTenantId()));

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<CampaignDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate) =>
        await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto dto)
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

    [HttpPost("{id}/launch")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Launch(Guid id)
    {
        var ok = await _service.LaunchAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to launch campaign (must be Draft or Paused)" });
        return Ok(new MessageResponse { Message = "Campaign launched" });
    }

    [HttpPost("{id}/pause")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Pause(Guid id)
    {
        var ok = await _service.PauseAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to pause campaign (must be Active)" });
        return Ok(new MessageResponse { Message = "Campaign paused" });
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ok = await _service.CompleteAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to complete campaign (must be Active or Paused)" });
        return Ok(new MessageResponse { Message = "Campaign completed" });
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to cancel campaign" });
        return Ok(new MessageResponse { Message = "Campaign cancelled" });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
