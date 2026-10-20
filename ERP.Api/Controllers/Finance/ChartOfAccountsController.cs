using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/chartofaccounts")]
[Authorize]
[PermissionResource("finance.chartOfAccounts")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ChartOfAccountsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<ChartOfAccountsController> _logger;

    public ChartOfAccountsController(IAccountingService accountingService, ILogger<ChartOfAccountsController> logger) : base(logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> AddChartOfAccount([FromBody] CreateChartOfAccountDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        
        var accountId = await _accountingService.AddChartOfAccountAsync(dto, tenantId, userId);
        return Ok(new { AccountId = accountId });
    }

    [HttpPut("{accountId}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateChartOfAccount(Guid accountId, [FromBody] UpdateChartOfAccountDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        
        dto.AccountId = accountId;
        var updatedAccountId = await _accountingService.UpdateChartOfAccountAsync(dto, tenantId, userId);
        return Ok(new { AccountId = updatedAccountId });
    }

    [HttpDelete("{accountId}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> DeleteChartOfAccount(Guid accountId)
    {
        var tenantId = GetTenantId();
        
        var result = await _accountingService.DeleteChartOfAccountAsync(accountId, tenantId);
        return Ok(new { Success = result });
    }

    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(ChartOfAccountDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetChartOfAccountById(Guid accountId)
    {
        var tenantId = GetTenantId();
        
        var account = await _accountingService.GetChartOfAccountByIdAsync(accountId, tenantId);
        if (account == null)
            return NotFound();
            
        return Ok(account);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChartOfAccountDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllChartOfAccounts(
        [FromQuery] string? accountType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int? level = null,
        [FromQuery] Guid? parentAccountId = null,
        [FromQuery] bool? postingOnly = null)
    {
        var tenantId = GetTenantId();
        var accounts = await _accountingService.GetAllChartOfAccountsAsync(
            tenantId, accountType, isActive, level, parentAccountId, postingOnly);
        return Ok(accounts);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}