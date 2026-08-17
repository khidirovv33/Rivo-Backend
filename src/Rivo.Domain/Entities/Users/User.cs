using Rivo.Domain.Common;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Users;

public class User : BaseEntity, ITenantEntity, ISoftDelete, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public UserStatus Status { get; set; } = UserStatus.PendingVerification;

    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
