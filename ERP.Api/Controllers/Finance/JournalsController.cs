using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/journals")]
[Authorize]
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
    public async Task<IActionResult> Create([FromBody] JournalCreateDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.AddJournalAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { JournalId = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var journal = await _accountingService.GetJournalByIdAsync(id, tenantId);
        if (journal == null)
            return NotFound();
        return Ok(journal);
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] JournalQuery query)
    {
        var tenantId = GetTenantId();
        query.TenantId = tenantId;
        var result = await _accountingService.SearchJournalsAsync(query);
        return Ok(new { Journals = result.Journals, TotalCount = result.TotalCount });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JournalUpdateDto dto)
    {
        dto.JournalId = id;

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.UpdateJournalAsync(dto, tenantId, userId);
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.PostJournalAsync(id, tenantId, userId);
        return Ok(new { JournalId = result });
    }

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] ReverseJournalDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var reason = dto.ReversalReason ?? dto.Reason;
        var result = await _accountingService.ReverseJournalAsync(id, tenantId, userId, reason);
        return Ok(new { JournalId = result });
    }

    [HttpGet("{id}/entries")]
    public async Task<IActionResult> GetEntries(Guid id)
    {
        var tenantId = GetTenantId();
        var entries = await _accountingService.GetJournalEntriesByJournalIdAsync(id, tenantId);
        return Ok(entries);
    }

    [HttpPost("{id}/entries")]
    public async Task<IActionResult> AddEntry(Guid id, [FromBody] CreateJournalEntryDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _accountingService.AddJournalEntryAsync(dto, id, tenantId, userId);
        return CreatedAtAction(nameof(GetEntries), new { id }, new { EntryId = result });
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}