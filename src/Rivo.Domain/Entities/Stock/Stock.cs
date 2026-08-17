using Rivo.Domain.Common;
using Rivo.Domain.Entities.Warehouses;

namespace Rivo.Domain.Entities.Stock;

/// <summary>Остаток товара на конкретном складе: System (учётный), Reserved, Available = System - Reserved.</summary>
public class Stock : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>FK -> Product (модуль Dev1).</summary>
    public Guid ProductId { get; set; }

    /// <summary>FK -> ProductVariation (модуль Dev1), опционально.</summary>
    public Guid? ProductVariationId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal ReservedQuantity { get; set; }

    public decimal AvailableQuantity => SystemQuantity - ReservedQuantity;
}
