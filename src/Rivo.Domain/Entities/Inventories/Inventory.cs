using Rivo.Domain.Common;
using Rivo.Domain.Entities.InventoryItems;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Inventories;

/// <summary>
/// Ревизия — одна из главных функций Rivo (раздел 11 ТЗ). Session-сущность: склад, ответственный,
/// дата, статус. Позиции — в InventoryItem. После Approve каждая позиция с разницей создаёт
/// корректирующий StockMovement (Adjustment) через IStockMovementsService.
/// </summary>
public class Inventory : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public InventoryStatus Status { get; set; } = InventoryStatus.Draft;

    public Guid ResponsibleUserId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
}
