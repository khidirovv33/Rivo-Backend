using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.PurchaseOrders;

/// <summary>Заказ поставщику. Финансово фиксируется отдельно в Purchase после фактического Receiving.</summary>
public class PurchaseOrder : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid SupplierId { get; set; }

    public Guid WarehouseId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
