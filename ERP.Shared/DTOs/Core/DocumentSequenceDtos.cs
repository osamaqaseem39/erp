namespace ERP.Shared.DTOs.Core;

public class DocumentSequenceDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int Padding { get; set; }
    /// <summary>Never or Yearly.</summary>
    public string ResetPolicy { get; set; } = string.Empty;
    /// <summary>The number the next document will get.</summary>
    public long NextNumber { get; set; }
    /// <summary>What the next document number will look like, e.g. QT-2026-00042.</summary>
    public string Preview { get; set; } = string.Empty;
}

public class UpdateDocumentSequenceDto
{
    public string Prefix { get; set; } = string.Empty;
    public int Padding { get; set; } = 5;
    public string ResetPolicy { get; set; } = "Yearly";
    public long NextNumber { get; set; } = 1;
}
