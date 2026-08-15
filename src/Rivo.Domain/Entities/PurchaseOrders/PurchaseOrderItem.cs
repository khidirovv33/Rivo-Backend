using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.PurchaseOrders;

public class PurchaseOrderItem : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    /// <summary>Накопительно сколько уже получено по этой строке (для расчёта частичного получения).</summary>
    public decimal ReceivedQuantity { get; set; }
}
