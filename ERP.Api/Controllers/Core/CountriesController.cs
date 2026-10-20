using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

[ApiController]
[Route("api/core/countries")]
[Authorize]
[PermissionResource("core.countries")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CountriesController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICoreMasterService _service;

    public CountriesController(ICoreMasterService service, ILogger<CountriesController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CountryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null) =>
        Ok(await _service.ListCountriesAsync(HttpContext.RequireTenantId(), isActive));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CountryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetCountryAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CountryDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateCountryDto dto)
    {
        var row = await _service.CreateCountryAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id = row.Id }, row);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CountryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCountryDto dto) =>
        Ok(await _service.UpdateCountryAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteCountryAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return NoContent();
    }
}
