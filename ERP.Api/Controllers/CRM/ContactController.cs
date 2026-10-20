using ERP.Api.Errors;
using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/contacts")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ContactController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IContactService _service;
    private readonly CreateContactValidator _createValidator;
    private readonly UpdateContactValidator _updateValidator;

    public ContactController(
        IContactService service,
        CreateContactValidator createValidator,
        UpdateContactValidator updateValidator,
        ILogger<ContactController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContactDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(GetTenantId()));

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid contact." });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var result = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var ok = await _service.ActivateAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var ok = await _service.DeactivateAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new { Success = true }) : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, GetTenantId());
        return result ? NoContent() : NotFound();
    }

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByCustomer(Guid customerId) =>
        Ok(await _service.GetByCustomerAsync(customerId, GetTenantId()));

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm) =>
        Ok(await _service.SearchAsync(searchTerm, GetTenantId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
