using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using FluentValidation;
using ERP.Api.Validators.CRM;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/campaigns")]
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
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpGet("type/{campaignType}")]
    public async Task<IActionResult> GetByType(string campaignType)
    {
        return await HandleAsync(() => _service.GetByTypeAsync(campaignType));
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto dto)
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

    [HttpPost("{id}/launch")]
    public async Task<IActionResult> Launch(Guid id)
    {
        return await HandleAsync(() => _service.LaunchAsync(id));
    }

    [HttpPost("{id}/pause")]
    public async Task<IActionResult> Pause(Guid id)
    {
        return await HandleAsync(() => _service.PauseAsync(id));
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        return await HandleAsync(() => _service.CompleteAsync(id));
    }
}