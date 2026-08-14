using Rivo.Application.Payments.Dtos;

namespace Rivo.Application.Pos.Dtos;

public class CheckoutItemRequestDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariationId { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class CheckoutRequestDto
{
    public Guid StoreId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal OrderDiscountAmount { get; set; }
    public List<CheckoutItemRequestDto> Items { get; set; } = new();
    public List<CreatePaymentRequestDto> Payments { get; set; } = new();
}
