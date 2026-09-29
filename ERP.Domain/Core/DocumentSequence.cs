namespace ERP.Domain.Core;

/// <summary>
/// Per-tenant document numbering, e.g. key "crm.quote" with prefix "QT" yields QT-2026-00042.
/// NextNumber is only ever advanced by a single atomic UPDATE (see DocumentNumberService).
/// </summary>
public class DocumentSequence : BaseEntity
{
    public string SequenceKey { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    /// <summary>The number the next document will get.</summary>
    public long NextNumber { get; set; } = 1;
    /// <summary>Minimum digits, zero-padded (5 → 00042).</summary>
    public int Padding { get; set; } = 5;
    /// <summary>"Never" or "Yearly". Yearly sequences include the year and restart at 1 each January.</summary>
    public string ResetPolicy { get; set; } = "Yearly";
    public int? LastResetYear { get; set; }
}
