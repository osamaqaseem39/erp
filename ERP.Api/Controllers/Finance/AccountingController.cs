using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Api.Validators.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/accounting")]
[Authorize]
public class AccountingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAccountingService _service;
    private readonly CreateChartOfAccountValidator _createChartValidator;
    private readonly UpdateChartOfAccountValidator _updateChartValidator;

    public AccountingController(
        IAccountingService service,
        CreateChartOfAccountValidator createChartValidator,
        UpdateChartOfAccountValidator updateChartValidator,
        ILogger<AccountingController> logger) : base(logger)
    {
        _service = service;
        _createChartValidator = createChartValidator;
        _updateChartValidator = updateChartValidator;
    }

    // Chart of Accounts endpoints
    [HttpGet("chart-of-accounts")]
    public async Task<IActionResult> GetAllChartOfAccounts([FromQuery] string? accountType = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetAllChartOfAccountsAsync(tenantId, accountType, isActive);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("chart-of-accounts/{id}")]
    public async Task<IActionResult> GetChartOfAccountById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetChartOfAccountByIdAsync(id, tenantId);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("chart-of-accounts")]
    public async Task<IActionResult> CreateChartOfAccount([FromBody] CreateChartOfAccountDto dto)
    {
        try
        {
            var validationResult = await _createChartValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var id = await _service.AddChartOfAccountAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetChartOfAccountById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("chart-of-accounts/{id}")]
    public async Task<IActionResult> UpdateChartOfAccount(Guid id, [FromBody] UpdateChartOfAccountDto dto)
    {
        try
        {
            dto.AccountId = id;
            var validationResult = await _updateChartValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateChartOfAccountAsync(dto, tenantId, userId);
            return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("chart-of-accounts/{id}")]
    public async Task<IActionResult> DeleteChartOfAccount(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.DeleteChartOfAccountAsync(id, tenantId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // Journal endpoints
    [HttpGet("journals")]
    public async Task<IActionResult> SearchJournals([FromQuery] JournalQuery query)
    {
        try
        {
            var result = await _service.SearchJournalsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("journals/{id}")]
    public async Task<IActionResult> GetJournalById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetJournalByIdAsync(id, tenantId);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("journals")]
    public async Task<IActionResult> CreateJournal([FromBody] JournalCreateDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var id = await _service.AddJournalAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetJournalById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("journals/{id}")]
    public async Task<IActionResult> UpdateJournal(Guid id, [FromBody] JournalUpdateDto dto)
    {
        try
        {
            dto.JournalId = id;
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateJournalAsync(dto, tenantId, userId);
            return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("journals/{id}/post")]
    public async Task<IActionResult> PostJournal(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var postedBy = GetUserId();
            var result = await _service.PostJournalAsync(id, tenantId, postedBy);
            return result != Guid.Empty ? Ok(new { id = result }) : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("journals/{id}/reverse")]
    public async Task<IActionResult> ReverseJournal(Guid id, [FromBody] ReverseJournalDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var reversedBy = GetUserId();
            var result = await _service.ReverseJournalAsync(id, tenantId, reversedBy, dto.ReversalReason);
            return result != Guid.Empty ? Ok(new { id = result }) : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // Journal Entries endpoints
    [HttpGet("journals/{journalId}/entries")]
    public async Task<IActionResult> GetJournalEntries(Guid journalId)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetJournalEntriesByJournalIdAsync(journalId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("journals/{journalId}/entries")]
    public async Task<IActionResult> CreateJournalEntry(Guid journalId, [FromBody] CreateJournalEntryDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var id = await _service.AddJournalEntryAsync(dto, journalId, tenantId, userId);
            return CreatedAtAction(nameof(GetJournalEntries), new { journalId }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("journal-entries/{id}")]
    public async Task<IActionResult> UpdateJournalEntry(Guid id, [FromBody] UpdateJournalEntryDto dto)
    {
        try
        {
            dto.EntryId = id;
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateJournalEntryAsync(dto, tenantId, userId);
            return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("journal-entries/{id}")]
    public async Task<IActionResult> DeleteJournalEntry(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.DeleteJournalEntryAsync(id, tenantId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // Financial Reports endpoints
    [HttpGet("reports/trial-balance")]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetTrialBalanceAsync(tenantId, fromDate, toDate, currencyCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("reports/general-ledger/{accountId}")]
    public async Task<IActionResult> GetGeneralLedgerBalance(Guid accountId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetGeneralLedgerBalanceAsync(accountId, tenantId, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    private Guid GetTenantId()
    {
        // Implementation to get tenant ID from context
        return Guid.NewGuid(); // Placeholder
    }

    private Guid GetUserId()
    {
        // Implementation to get user ID from context
        return Guid.NewGuid(); // Placeholder
    }
}