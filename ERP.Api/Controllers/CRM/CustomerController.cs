using ERP.Api.Errors;
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
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _service.GetByIdAsync(id);
        return customer is null ? NotFound(NotFoundError(id)) : Ok(customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    /// <summary>Server-side paged, filtered and sorted customer list.</summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] CustomerPagedQuery query, CancellationToken cancellationToken)
    {
        return await HandleAsync(() => _service.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        return await HandleAsync(() => _service.SearchAsync(searchTerm));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpGet("industry/{industry}")]
    public async Task<IActionResult> GetByIndustry(string industry)
    {
        return await HandleAsync(() => _service.GetByIndustryAsync(industry));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        // HandleAsync cannot return a Guid body (its T is class-constrained), so respond explicitly.
        var id = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await _service.UpdateAsync(dto) ? NoContent() : NotFound(NotFoundError(id));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound(NotFoundError(id));
    }

    private ApiError NotFoundError(Guid id) => new()
    {
        Status = 404,
        ErrorCode = ApiErrorCodes.NotFound,
        Message = "This customer could not be found. It may have been deleted.",
        TraceId = HttpContext.TraceIdentifier,
    };
}
