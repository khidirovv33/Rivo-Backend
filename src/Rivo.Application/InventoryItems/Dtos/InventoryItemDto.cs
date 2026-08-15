namespace Rivo.Application.InventoryItems.Dtos;

public class InventoryItemDto
{
    public Guid Id { get; set; }

    public Guid InventoryId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal ActualQuantity { get; set; }

    public decimal Difference { get; set; }

    public decimal UnitCost { get; set; }

    public decimal DifferenceCost { get; set; }
}

/// <summary>Скан товара + ввод фактического количества. SystemQuantity/UnitCost берутся сервером из Stock на момент скана.</summary>
public class ScanInventoryItemDto
{
    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal ActualQuantity { get; set; }

    /// <summary>Опционально — если не задано, берётся из последней закупочной цены/0.</summary>
    public decimal? UnitCost { get; set; }
}
