using Microsoft.AspNetCore.Mvc;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/chartofaccounts")]
[Authorize]
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
    public async Task<IActionResult> AddChartOfAccount([FromBody] CreateChartOfAccountDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        
        var accountId = await _accountingService.AddChartOfAccountAsync(dto, tenantId, userId);
        return Ok(new { AccountId = accountId });
    }

    [HttpPut("{accountId}")]
    public async Task<IActionResult> UpdateChartOfAccount(Guid accountId, [FromBody] UpdateChartOfAccountDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        
        dto.AccountId = accountId;
        var updatedAccountId = await _accountingService.UpdateChartOfAccountAsync(dto, tenantId, userId);
        return Ok(new { AccountId = updatedAccountId });
    }

    [HttpDelete("{accountId}")]
    public async Task<IActionResult> DeleteChartOfAccount(Guid accountId)
    {
        var tenantId = GetTenantId();
        
        var result = await _accountingService.DeleteChartOfAccountAsync(accountId, tenantId);
        return Ok(new { Success = result });
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetChartOfAccountById(Guid accountId)
    {
        var tenantId = GetTenantId();
        
        var account = await _accountingService.GetChartOfAccountByIdAsync(accountId, tenantId);
        if (account == null)
            return NotFound();
            
        return Ok(account);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChartOfAccounts([FromQuery] string? accountType = null, [FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        
        var accounts = await _accountingService.GetAllChartOfAccountsAsync(tenantId, accountType, isActive);
        return Ok(accounts);
    }

    private Guid GetTenantId()
    {
        // Extract tenant ID from JWT token or context
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        // Extract user ID from JWT token or context
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}