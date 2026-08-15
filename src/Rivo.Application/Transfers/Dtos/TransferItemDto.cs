namespace Rivo.Application.Transfers.Dtos;

public class TransferItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }
}

public class CreateTransferItemDto
{
    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }
}
