using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Receiving;

/// <summary>Факт поступления товара по PurchaseOrder (полное или частичное). Каждая позиция создаёт StockMovement (Receipt).</summary>
public class Receiving : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid WarehouseId { get; set; }

    public DateTime ReceivingDate { get; set; }

    public ReceivingStatus Status { get; set; } = ReceivingStatus.Draft;

    public string? Notes { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public ICollection<ReceivingItem> Items { get; set; } = new List<ReceivingItem>();
}
