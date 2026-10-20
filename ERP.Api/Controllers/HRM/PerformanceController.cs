using ERP.Api.Errors;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/performance")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PerformanceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPerformanceService _service;
    private readonly CreatePerformanceReviewValidator _createValidator;
    private readonly UpdatePerformanceReviewValidator _updateValidator;

    public PerformanceController(
        IPerformanceService service,
        CreatePerformanceReviewValidator createValidator,
        UpdatePerformanceReviewValidator updateValidator,
        ILogger<PerformanceController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PerformanceReviewDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PerformanceReviewDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(GetTenantId()));

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceReviewDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId) =>
        Ok(await _service.GetByEmployeeAsync(employeeId, GetTenantId()));

    [HttpGet("reviewer/{reviewerId}")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceReviewDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByReviewer(Guid reviewerId) =>
        Ok(await _service.GetByReviewerAsync(reviewerId, GetTenantId()));

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceReviewDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByPeriod(DateTime startDate, DateTime endDate) =>
        Ok(await _service.GetByPeriodAsync(startDate, endDate, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreatePerformanceReviewDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePerformanceReviewDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await _service.UpdateAsync(dto, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Updated" })
            : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, GetTenantId()) ? NoContent() : NotFound();

    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var ok = await _service.SubmitAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new MessageResponse { Message = "Submitted" }) : NotFound();
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _service.ApproveAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new MessageResponse { Message = "Approved" }) : NotFound();
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ok = await _service.CompleteAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new MessageResponse { Message = "Completed" }) : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
