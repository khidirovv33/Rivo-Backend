namespace Rivo.Application.Receiving.Dtos;

public class ReceivingItemDto
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderItemId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal QuantityReceived { get; set; }

    public decimal UnitCost { get; set; }
}

public class CreateReceivingItemDto
{
    public Guid PurchaseOrderItemId { get; set; }

    public decimal QuantityReceived { get; set; }

    /// <summary>Если не задано — берётся UnitCost из строки заказа.</summary>
    public decimal? UnitCost { get; set; }
}
