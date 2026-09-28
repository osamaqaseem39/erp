using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class QuoteDto : BaseDto
{
    public string? QuoteNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OpportunityId { get; set; }
    public DateTime? QuoteDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Status { get; set; } = "Draft";
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public new bool? IsActive { get; set; } = true;
    public string? CustomerName { get; set; }
    public string? OpportunityName { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid? CompanyId { get; set; }
    public List<QuoteLineDto> QuoteLines { get; set; } = new();
}

public class CreateQuoteDto : CreateDto
{
    public string? QuoteNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OpportunityId { get; set; }
    public DateTime? QuoteDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Status { get; set; } = "Draft";
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public List<CreateQuoteLineDto> QuoteLines { get; set; } = new();
}

public class UpdateQuoteDto : UpdateDto
{
    public string? QuoteNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OpportunityId { get; set; }
    public DateTime? QuoteDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Status { get; set; } = "Draft";
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public List<CreateQuoteLineDto> QuoteLines { get; set; } = new();
}