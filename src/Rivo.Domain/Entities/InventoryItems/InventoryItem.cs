using Rivo.Domain.Common;
using Rivo.Domain.Entities.Inventories;

namespace Rivo.Domain.Entities.InventoryItems;

/// <summary>Одна позиция ревизии: системное количество (снимок на момент сканирования) vs фактическое.</summary>
public class InventoryItem : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid InventoryId { get; set; }

    public Inventory Inventory { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal ActualQuantity { get; set; }

    public decimal Difference => ActualQuantity - SystemQuantity;

    /// <summary>Себестоимость единицы на момент ревизии — для расчёта стоимости расхождения.</summary>
    public decimal UnitCost { get; set; }

    public decimal DifferenceCost => Difference * UnitCost;
}
