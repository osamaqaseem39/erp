using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class ReverseJournalDto
{
    public Guid JournalEntryId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReversalReason { get; set; }
    public string? Notes { get; set; }
}