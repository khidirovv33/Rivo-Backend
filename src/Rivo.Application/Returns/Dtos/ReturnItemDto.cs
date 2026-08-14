namespace Rivo.Application.Returns.Dtos;

public class ReturnItemDto
{
    public Guid Id { get; set; }
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }
}

public class CreateReturnItemRequestDto
{
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
}

public class CreateReturnRequestDto
{
    public Guid OrderId { get; set; }
    public string? Reason { get; set; }
    public List<CreateReturnItemRequestDto> Items { get; set; } = new();
}
