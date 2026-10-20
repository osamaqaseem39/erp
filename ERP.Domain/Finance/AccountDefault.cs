using ERP.Domain;

namespace ERP.Domain.Finance;

/// <summary>Maps a system posting purpose (Cash, AR, …) to a CoA account per tenant.</summary>
public class AccountDefault : BaseEntity
{
    public Guid AccountDefaultId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public new bool IsActive { get; set; } = true;

    public ChartOfAccount? Account { get; set; }
}
