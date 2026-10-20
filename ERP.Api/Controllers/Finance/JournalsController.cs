using ERP.Api.Security;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/journals")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class JournalsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<JournalsController> _logger;

    public JournalsController(IAccountingService accountingService, ILogger<JournalsController> logger) : base(logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] JournalCreateDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.AddJournalAsync(dto, tenantId, userId);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Could not create journal." });
        return CreatedAtAction(nameof(GetById), new { id = result }, new { JournalId = result });
    }

    /// <summary>Last-used cash/bank account and cost center for typed voucher forms.</summary>
    [HttpGet("voucher-defaults")]
    [ProducesResponseType(typeof(VoucherDefaultsDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetVoucherDefaults()
        => Ok(await _accountingService.GetVoucherDefaultsAsync(GetTenantId()));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JournalDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var journal = await _accountingService.GetJournalByIdAsync(id, tenantId);
        if (journal == null)
            return NotFound();
        return Ok(journal);
    }

    [HttpGet]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Search([FromQuery] JournalQuery query)
    {
        var tenantId = GetTenantId();
        query.TenantId = tenantId;
        var result = await _accountingService.SearchJournalsAsync(query);
        return Ok(new { Journals = result.Journals, TotalCount = result.TotalCount });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] JournalUpdateDto dto)
    {
        dto.JournalId = id;

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.UpdateJournalAsync(dto, tenantId, userId);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Could not update journal (not found or not editable)." });
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.ApproveJournalAsync(id, tenantId, userId);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Cannot approve (must be balanced Draft; approver must differ from creator)." });
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/reject")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReverseJournalDto? dto = null)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.RejectJournalAsync(id, tenantId, userId, dto?.ReversalReason ?? dto?.Reason);
        if (result == Guid.Empty) return BadRequest();
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/post")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Post(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.PostJournalAsync(id, tenantId, userId);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Cannot post journal (must be balanced Approved/Draft, open period, and within budget when finance.enforceBudget is on)." });
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/unpost")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Unpost(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.UnpostJournalAsync(id, tenantId, userId);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Cannot unpost (must be Posted)." });
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/reverse")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] ReverseJournalDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var reason = dto.ReversalReason ?? dto.Reason;
        var result = await _accountingService.ReverseJournalAsync(id, tenantId, userId, reason);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Cannot reverse journal." });
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/cheque-status")]
    [ProducesResponseType(typeof(JournalDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SetChequeStatus(Guid id, [FromBody] SetChequeStatusDto dto)
    {
        var ok = await _accountingService.SetChequeStatusAsync(id, dto, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new { error = "Could not update cheque status (missing cheque no or invalid status)." });
        return Ok(await _accountingService.GetJournalByIdAsync(id, GetTenantId()));
    }

    [HttpGet("{id}/entries")]
    [ProducesResponseType(typeof(IEnumerable<JournalEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetEntries(Guid id)
    {
        var tenantId = GetTenantId();
        var entries = await _accountingService.GetJournalEntriesByJournalIdAsync(id, tenantId);
        return Ok(entries);
    }

    [HttpPost("{id}/entries")]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> AddEntry(Guid id, [FromBody] CreateJournalEntryDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.AddJournalEntryAsync(dto, id, tenantId, userId);
        if (result == Guid.Empty)
            return BadRequest(new { error = "Could not add journal entry." });
        return CreatedAtAction(nameof(GetEntries), new { id }, new { EntryId = result });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}