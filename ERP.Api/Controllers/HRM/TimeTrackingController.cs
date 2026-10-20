using ERP.Api.Errors;
using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Shared.DTOs.HRM;
using ERP.Application.Interfaces.HRM;
using ERP.Api.Validators.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/time-tracking")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class TimeTrackingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITimeTrackingService _service;
    private readonly CreateTimeEntryValidator _createValidator;
    private readonly UpdateTimeEntryValidator _updateValidator;

    public TimeTrackingController(
        ITimeTrackingService service,
        CreateTimeEntryValidator createValidator,
        UpdateTimeEntryValidator updateValidator,
        ILogger<TimeTrackingController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TimeEntryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
        => await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
        => await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
        => await HandleAsync(() => _service.GetByEmployeeAsync(employeeId, GetTenantId()));

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
        => await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate, GetTenantId()));

    [HttpGet("employee/{employeeId}/total-hours")]
    [ProducesResponseType(typeof(decimal), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTotalHours(Guid employeeId, DateTime startDate, DateTime endDate)
        => await HandleAsync(() => _service.GetTotalHoursAsync(employeeId, startDate, endDate, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(Guid), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        dto.TenantId = GetTenantId();
        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeEntryDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
        => await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
}
