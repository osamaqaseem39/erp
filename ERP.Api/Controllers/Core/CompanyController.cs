using ERP.Api.OpenApi;
using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

/// <summary>The signed-in user's company profile, and the formatting context the app loads after sign-in.</summary>
[ApiController]
[Route("api/core")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CompanyController : ControllerBase
{
    private readonly ICompanyProfileService _companies;

    public CompanyController(ICompanyProfileService companies)
    {
        _companies = companies;
    }

    [HttpGet("company")]
    [ProducesResponseType(typeof(CompanyProfileDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<CompanyProfileDto>> Get() =>
        Ok(await _companies.GetAsync(User.RequireCompanyId(), HttpContext.RequireTenantId()));

    [HttpPut("company")]
    [ProducesResponseType(typeof(CompanyProfileDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<CompanyProfileDto>> Update([FromBody] UpdateCompanyProfileDto dto) =>
        Ok(await _companies.UpdateAsync(User.RequireCompanyId(), HttpContext.RequireTenantId(), dto));

    /// <summary>Currency, time zone, locale and date format for the current tenant. Works even without a company.</summary>
    [HttpGet("context")]
    [SignedInOnly("Company name, currency and formats every screen needs.")]
    [ProducesResponseType(typeof(TenantContextDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<TenantContextDto>> Context()
    {
        Guid? companyId = Guid.TryParse(User.FindFirst(CurrentUser.CompanyIdClaim)?.Value, out var id) ? id : null;
        return Ok(await _companies.GetTenantContextAsync(companyId, HttpContext.RequireTenantId()));
    }
}
