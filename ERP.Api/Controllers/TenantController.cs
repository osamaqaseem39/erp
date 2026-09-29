using ERP.Api.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Api.Validators;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/tenants")]
// Platform administration: SuperAdmin only. The two sign-in lookups below opt out with [AllowAnonymous].
[Authorize(Roles = "SuperAdmin,SUPER_ADMIN")]
public class TenantController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITenantApiService _tenantService;
    private readonly CreateTenantValidator _createValidator;
    private readonly UpdateTenantValidator _updateValidator;

    public TenantController(
        ITenantApiService tenantService,
        CreateTenantValidator createValidator,
        UpdateTenantValidator updateValidator,
        ILogger<TenantController> logger) : base(logger)
    {
        _tenantService = tenantService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _tenantService.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _tenantService.GetAllAsync());
    }

    /// <summary>
    /// Public active-tenant list for the login tenant picker (id/name/domain only).
    /// </summary>
    [HttpGet("login-options")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLoginOptions()
    {
        return await HandleAsync(() => _tenantService.GetLoginOptionsAsync());
    }

    /// <summary>
    /// Public tenant lookup by domain for login UI (connection string never included).
    /// </summary>
    [HttpGet("domain/{domain}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByDomain(string domain)
    {
        return await HandleAsync(() => _tenantService.GetByDomainAsync(domain));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _tenantService.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _tenantService.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _tenantService.DeleteAsync(id));
    }
}
