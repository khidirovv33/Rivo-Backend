using Rivo.Domain.Common;
using Rivo.Domain.Entities.Products;

namespace Rivo.Domain.Entities.Orders;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductVariationId { get; set; }
    public ProductVariation? ProductVariation { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}
