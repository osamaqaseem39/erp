using ERP.Domain;

namespace ERP.Domain.CRM;

/// <summary>Shared packaging specification fields on enquiry / pre-cost lines.</summary>
public abstract class PackagingLineFields : BaseEntity
{
    public string? Size { get; set; }
    public string? MaterialType { get; set; }
    public decimal? PET { get; set; }
    public decimal? MPET { get; set; }
    public decimal? LaminationMicron { get; set; }
    public decimal? CutOffLength { get; set; }
    public decimal? FolderSize { get; set; }
    public int? Ups { get; set; }
    public Guid? PouchTypeId { get; set; }
    public Guid? ProcessId { get; set; }
    public Guid? FOHTypeId { get; set; }
}
