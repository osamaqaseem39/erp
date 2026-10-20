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
[Route("api/hrm/training")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class TrainingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITrainingService _service;
    private readonly CreateTrainingValidator _createValidator;
    private readonly UpdateTrainingValidator _updateValidator;

    public TrainingController(
        ITrainingService service,
        CreateTrainingValidator createValidator,
        UpdateTrainingValidator updateValidator,
        ILogger<TrainingController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TrainingDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(GetTenantId()));

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<TrainingDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId) =>
        Ok(await _service.GetByEmployeeAsync(employeeId, GetTenantId()));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<TrainingDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status) =>
        Ok(await _service.GetByStatusAsync(status, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateTrainingDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainingDto dto)
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

    [HttpPost("{id}/enroll")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> EnrollEmployee(Guid id, [FromBody] EnrollEmployeeDto dto)
    {
        var ok = await _service.EnrollEmployeeAsync(id, dto.EmployeeId, GetTenantId(), GetUserId());
        return ok ? Ok(new MessageResponse { Message = "Enrolled" }) : NotFound();
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CompleteTraining(Guid id, [FromBody] CompleteTrainingDto dto)
    {
        var ok = await _service.CompleteTrainingAsync(
            id, dto.EmployeeId, dto.Score ?? 0, GetTenantId(), GetUserId());
        return ok ? Ok(new MessageResponse { Message = "Completed" }) : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
