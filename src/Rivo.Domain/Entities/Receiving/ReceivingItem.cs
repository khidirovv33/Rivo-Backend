using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Receiving;

public class ReceivingItem : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid ReceivingId { get; set; }

    public Receiving Receiving { get; set; } = null!;

    public Guid PurchaseOrderItemId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal QuantityReceived { get; set; }

    public decimal UnitCost { get; set; }
}
