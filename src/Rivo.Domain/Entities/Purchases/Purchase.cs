using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Purchases;

/// <summary>
/// Финансовая запись о закупке — создаётся автоматически при завершении Receiving.
/// Источник для "истории закупок и задолженности перед поставщиком" (раздел 9 ТЗ).
/// </summary>
public class Purchase : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid SupplierId { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid ReceivingId { get; set; }

    public DateTime PurchaseDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal OutstandingAmount => TotalAmount - PaidAmount;

    public string? Notes { get; set; }
}
