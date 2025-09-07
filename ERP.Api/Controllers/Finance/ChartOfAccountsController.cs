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
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            
            var accountId = await _accountingService.AddChartOfAccountAsync(dto, tenantId, userId);
            return Ok(new { AccountId = accountId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding chart of account");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{accountId}")]
    public async Task<IActionResult> UpdateChartOfAccount(Guid accountId, [FromBody] UpdateChartOfAccountDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            
            dto.AccountId = accountId;
            var updatedAccountId = await _accountingService.UpdateChartOfAccountAsync(dto, tenantId, userId);
            return Ok(new { AccountId = updatedAccountId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chart of account {AccountId}", accountId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{accountId}")]
    public async Task<IActionResult> DeleteChartOfAccount(Guid accountId)
    {
        try
        {
            var tenantId = GetTenantId();
            
            var result = await _accountingService.DeleteChartOfAccountAsync(accountId, tenantId);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart of account {AccountId}", accountId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetChartOfAccountById(Guid accountId)
    {
        try
        {
            var tenantId = GetTenantId();
            
            var account = await _accountingService.GetChartOfAccountByIdAsync(accountId, tenantId);
            if (account == null)
                return NotFound();
                
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart of account {AccountId}", accountId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChartOfAccounts([FromQuery] string? accountType = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var tenantId = GetTenantId();
            
            var accounts = await _accountingService.GetAllChartOfAccountsAsync(tenantId, accountType, isActive);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all chart of accounts");
            return BadRequest(new { Error = ex.Message });
        }
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