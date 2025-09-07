using ERP.Domain;

namespace ERP.Domain.Core;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}