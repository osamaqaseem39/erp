using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class JournalDto : BaseDto
{
    public Guid? JournalId { get; set; }
    public new Guid? TenantId { get; set; }
    public string? JournalType { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? JournalDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public string? Status { get; set; }
    public new DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool? IsLocked { get; set; }
    public List<JournalEntryDto>? JournalEntries { get; set; }
}


public class JournalEntryDto : BaseDto
{
    public Guid EntryId { get; set; }
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
}

public class CreateJournalDto : CreateDto
{
    public string JournalType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public DateTime JournalDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsLocked { get; set; } = false;
}

public class UpdateJournalDto : UpdateDto
{
    public Guid? JournalId { get; set; }
    public string? JournalType { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? JournalDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? Status { get; set; }
    public bool? IsLocked { get; set; }
}

public class CreateJournalEntryDto : CreateDto
{
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
}

public class UpdateJournalEntryDto : UpdateDto
{
    public Guid? EntryId { get; set; }
    public Guid? AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public string? Narration { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
}


public class JournalQuery
{
    public Guid TenantId { get; set; }
    public string? JournalType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// Aliases for interface compatibility
public class JournalCreateDto : CreateJournalDto { }
public class JournalUpdateDto : UpdateJournalDto { }