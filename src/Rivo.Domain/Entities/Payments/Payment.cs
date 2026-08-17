using Rivo.Domain.Common;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Payments;

public class Payment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
