using Rivo.Domain.Common;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Returns;

public class Return : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProcessedByUserId { get; set; }
    public User ProcessedByUser { get; set; } = null!;

    public string? Reason { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Completed;

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}
