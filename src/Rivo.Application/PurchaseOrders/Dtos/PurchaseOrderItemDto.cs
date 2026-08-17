namespace Rivo.Application.PurchaseOrders.Dtos;

public class PurchaseOrderItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal RemainingQuantity => Quantity - ReceivedQuantity;
}

public class CreatePurchaseOrderItemDto
{
    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }
}
