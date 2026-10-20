using ERP.Api.Errors;
using ERP.Api.OpenApi;
using ERP.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;
using FluentValidation;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/customers")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CustomerController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICustomerService _service;
    private readonly CreateCustomerValidator _createValidator;
    private readonly UpdateCustomerValidator _updateValidator;

    public CustomerController(
        ICustomerService service,
        CreateCustomerValidator createValidator,
        UpdateCustomerValidator updateValidator,
        ILogger<CustomerController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _service.GetByIdAsync(id, GetTenantId());
        return customer is null ? NotFound(NotFoundError(id)) : Ok(customer);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync(GetTenantId()));
    }

    /// <summary>Server-side paged, filtered and sorted customer list.</summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetPaged([FromQuery] CustomerPagedQuery query, CancellationToken cancellationToken)
    {
        return await HandleAsync(() => _service.GetPagedAsync(query, GetTenantId(), cancellationToken));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        return await HandleAsync(() => _service.SearchAsync(searchTerm, GetTenantId()));
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status, GetTenantId()));
    }

    [HttpGet("industry/{industry}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByIndustry(string industry)
    {
        return await HandleAsync(() => _service.GetByIndustryAsync(industry, GetTenantId()));
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid customer." });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await _service.UpdateAsync(dto, GetTenantId(), GetUserId())
            ? NoContent()
            : NotFound(NotFoundError(id));
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var ok = await _service.ActivateAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new { Success = true }) : NotFound(NotFoundError(id));
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var ok = await _service.DeactivateAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new { Success = true }) : NotFound(NotFoundError(id));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id, GetTenantId())
            ? NoContent()
            : NotFound(NotFoundError(id));
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();

    private ApiError NotFoundError(Guid id) => new()
    {
        Status = 404,
        ErrorCode = ApiErrorCodes.NotFound,
        Message = "This customer could not be found. It may have been deleted.",
        TraceId = HttpContext.TraceIdentifier,
    };
}
