namespace ERP.Domain.Core;

public class Branch : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Guid? CityId { get; set; }
}
