using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/account-defaults")]
[Authorize]
[PermissionResource("finance.accountDefaults")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class AccountDefaultsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAccountDefaultService _service;

    public AccountDefaultsController(IAccountDefaultService service, ILogger<AccountDefaultsController> logger)
        : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDefaultDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync(GetTenantId());
        return Ok(result);
    }

    [HttpGet("{purpose}")]
    [ProducesResponseType(typeof(AccountDefaultDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByPurpose(string purpose)
    {
        var result = await _service.GetByPurposeAsync(purpose, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<AccountDefaultDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Upsert([FromBody] UpsertAccountDefaultsDto dto)
    {
        var result = await _service.UpsertAsync(dto, GetTenantId(), GetUserId());
        return Ok(result);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
