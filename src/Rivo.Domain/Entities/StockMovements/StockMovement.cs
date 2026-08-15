using Rivo.Domain.Common;
using Rivo.Domain.Entities.Warehouses;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.StockMovements;

/// <summary>Полная история складских операций (раздел 8 ТЗ). Единственный путь изменения Stock.SystemQuantity.</summary>
public class StockMovement : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public StockMovementType Type { get; set; }

    /// <summary>Знаковая дельта: положительная — приход, отрицательная — расход.</summary>
    public decimal Quantity { get; set; }

    public decimal QuantityBefore { get; set; }

    public decimal QuantityAfter { get; set; }

    public string? Reason { get; set; }

    /// <summary>Логическая ссылка на источник операции (например "Purchase", "Sale", "Transfer", "Inventory").</summary>
    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}
