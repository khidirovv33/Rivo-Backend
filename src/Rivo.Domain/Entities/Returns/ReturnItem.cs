using Rivo.Domain.Common;
using Rivo.Domain.Entities.Orders;

namespace Rivo.Domain.Entities.Returns;

public class ReturnItem : BaseEntity
{
    public Guid ReturnId { get; set; }
    public Return Return { get; set; } = null!;

    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }
}
