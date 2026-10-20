using ERP.Api.OpenApi;
using ERP.Application.Core.Settings;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

/// <summary>Typed tenant settings, one category at a time (preferences, security, email).</summary>
[ApiController]
[Route("api/core/settings")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CoreSettingsController : ControllerBase
{
    private readonly ICoreSettingsService _settings;

    public CoreSettingsController(ICoreSettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet("{category}")]
    [ProducesResponseType(typeof(SettingsCategoryDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<SettingsCategoryDto>> Get(string category) => Ok(await _settings.GetCategoryAsync(category));

    [HttpPut("{category}")]
    [ProducesResponseType(typeof(SettingsCategoryDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<SettingsCategoryDto>> Update(string category, [FromBody] UpdateSettingsCategoryDto dto) =>
        Ok(await _settings.UpdateCategoryAsync(category, dto));
}
