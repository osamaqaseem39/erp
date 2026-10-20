using ERP.Api.Errors;
using ERP.Api.OpenApi;
using ERP.Api.Services;
using ERP.Api.Validators;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/tenants")]
// Platform administration: SuperAdmin only. The two sign-in lookups below opt out with [AllowAnonymous].
[Authorize(Roles = "SuperAdmin,SUPER_ADMIN")]
[ProducesUnauthorized]
[ProducesForbidden]
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
    [ProducesResponseType(typeof(TenantDto), OpenApiResponseTypes.Ok)]
    [ProducesNotFound]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _tenantService.GetByIdAsync(id));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _tenantService.GetAllAsync());

    /// <summary>
    /// Public active-tenant list for the login tenant picker (id/name/domain only).
    /// </summary>
    [HttpGet("login-options")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<LoginTenantOptionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetLoginOptions() =>
        await HandleAsync(() => _tenantService.GetLoginOptionsAsync());

    /// <summary>
    /// Public tenant lookup by domain for login UI (connection string never included).
    /// </summary>
    [HttpGet("domain/{domain}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantDto), OpenApiResponseTypes.Ok)]
    [ProducesNotFound]
    public async Task<IActionResult> GetByDomain(string domain) =>
        await HandleAsync(() => _tenantService.GetByDomainAsync(domain));

    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), OpenApiResponseTypes.Ok)]
    [ProducesBadRequest]
    [ProducesConflict]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _tenantService.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TenantDto), OpenApiResponseTypes.Ok)]
    [ProducesBadRequest]
    [ProducesNotFound]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _tenantService.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    [ProducesNotFound]
    [ProducesConflict]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _tenantService.DeleteAsync(id));
}
