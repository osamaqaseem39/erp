using ERP.Api.Security;
using ERP.Api.OpenApi;
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
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
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
    [ProducesResponseType(typeof(IEnumerable<ChartOfAccountDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllChartOfAccounts(
        [FromQuery] string? accountType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int? level = null,
        [FromQuery] Guid? parentAccountId = null,
        [FromQuery] bool? postingOnly = null)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAllChartOfAccountsAsync(
            tenantId, accountType, isActive, level, parentAccountId, postingOnly);
        return Ok(result);
    }

    [HttpPost("chart-of-accounts/seed-defaults")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SeedDefaultChartOfAccounts()
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var count = await _service.SeedDefaultChartOfAccountsAsync(tenantId, userId);
        return Ok(new { seeded = count });
    }

    [HttpGet("chart-of-accounts/{id}")]
    [ProducesResponseType(typeof(ChartOfAccountDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetChartOfAccountById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetChartOfAccountByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("chart-of-accounts")]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateChartOfAccount([FromBody] CreateChartOfAccountDto dto)
    {
        var validationResult = await _createChartValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var id = await _service.AddChartOfAccountAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetChartOfAccountById), new { id }, new { id });
    }

    [HttpPut("chart-of-accounts/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateChartOfAccount(Guid id, [FromBody] UpdateChartOfAccountDto dto)
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

    [HttpDelete("chart-of-accounts/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteChartOfAccount(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.DeleteChartOfAccountAsync(id, tenantId);
        return result ? NoContent() : NotFound();
    }

    // Journal endpoints
    [HttpGet("journals")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SearchJournals([FromQuery] JournalQuery query)
    {
        var result = await _service.SearchJournalsAsync(query);
        return Ok(result);
    }

    [HttpGet("journals/{id}")]
    [ProducesResponseType(typeof(JournalDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetJournalById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetJournalByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("journals")]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateJournal([FromBody] JournalCreateDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var id = await _service.AddJournalAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetJournalById), new { id }, new { id });
    }

    [HttpPut("journals/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateJournal(Guid id, [FromBody] JournalUpdateDto dto)
    {
        dto.JournalId = id;
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UpdateJournalAsync(dto, tenantId, userId);
        return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
    }

    [HttpPost("journals/{id}/approve")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ApproveJournal(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.ApproveJournalAsync(id, tenantId, userId);
        return result != Guid.Empty ? Ok(new { id = result }) : BadRequest(new { error = "Cannot approve (must be balanced Draft; approver must differ from creator)." });
    }

    [HttpPost("journals/{id}/reject")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> RejectJournal(Guid id, [FromBody] ReverseJournalDto? dto = null)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.RejectJournalAsync(id, tenantId, userId, dto?.ReversalReason ?? dto?.Reason);
        return result != Guid.Empty ? Ok(new { id = result }) : BadRequest();
    }

    [HttpPost("journals/{id}/post")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PostJournal(Guid id)
    {
        var tenantId = GetTenantId();
        var postedBy = GetUserId();
        var result = await _service.PostJournalAsync(id, tenantId, postedBy);
        return result != Guid.Empty ? Ok(new { id = result }) : BadRequest();
    }

    [HttpPost("journals/{id}/unpost")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UnpostJournal(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UnpostJournalAsync(id, tenantId, userId);
        return result != Guid.Empty ? Ok(new { id = result }) : BadRequest(new { error = "Cannot unpost (must be Posted)." });
    }

    [HttpPost("journals/{id}/reverse")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ReverseJournal(Guid id, [FromBody] ReverseJournalDto dto)
    {
        var tenantId = GetTenantId();
        var reversedBy = GetUserId();
        var result = await _service.ReverseJournalAsync(id, tenantId, reversedBy, dto.ReversalReason);
        return result != Guid.Empty ? Ok(new { id = result }) : BadRequest();
    }

    // Journal Entries endpoints
    [HttpGet("journals/{journalId}/entries")]
    [ProducesResponseType(typeof(IEnumerable<JournalEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetJournalEntries(Guid journalId)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetJournalEntriesByJournalIdAsync(journalId, tenantId);
        return Ok(result);
    }

    [HttpPost("journals/{journalId}/entries")]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateJournalEntry(Guid journalId, [FromBody] CreateJournalEntryDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var id = await _service.AddJournalEntryAsync(dto, journalId, tenantId, userId);
        return CreatedAtAction(nameof(GetJournalEntries), new { journalId }, new { id });
    }

    [HttpPut("journal-entries/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateJournalEntry(Guid id, [FromBody] UpdateJournalEntryDto dto)
    {
        dto.EntryId = id;
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UpdateJournalEntryAsync(dto, tenantId, userId);
        return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
    }

    [HttpDelete("journal-entries/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteJournalEntry(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.DeleteJournalEntryAsync(id, tenantId);
        return result ? NoContent() : NotFound();
    }

    // Financial Reports endpoints (also exposed under /api/finance/reports/*)
    [HttpGet("reports/trial-balance")]
    [ProducesResponseType(typeof(TrialBalanceReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var result = await _service.GetTrialBalanceAsync(GetTenantId(), fromDate, toDate, currencyCode);
        return Ok(result);
    }

    [HttpGet("reports/running-trial-balance")]
    [ProducesResponseType(typeof(RunningTrialBalanceReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetRunningTrialBalance([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var result = await _service.GetRunningTrialBalanceAsync(GetTenantId(), fromDate, toDate, currencyCode);
        return Ok(result);
    }

    [HttpGet("reports/profit-loss")]
    [ProducesResponseType(typeof(ProfitAndLossReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetProfitAndLoss([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var result = await _service.GetProfitAndLossAsync(GetTenantId(), fromDate, toDate, currencyCode);
        return Ok(result);
    }

    [HttpGet("reports/balance-sheet")]
    [ProducesResponseType(typeof(BalanceSheetReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate, [FromQuery] string? currencyCode = null)
    {
        var result = await _service.GetBalanceSheetAsync(GetTenantId(), asOfDate, currencyCode);
        return Ok(result);
    }

    [HttpGet("reports/general-ledger/{accountId}")]
    [ProducesResponseType(typeof(LedgerReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetGeneralLedgerBalance(
        Guid accountId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] bool postingOnly = false)
    {
        var result = await _service.GetGeneralLedgerBalanceAsync(accountId, GetTenantId(), fromDate, toDate, postingOnly);
        return Ok(result);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}