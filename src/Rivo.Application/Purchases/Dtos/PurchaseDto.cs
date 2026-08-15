namespace Rivo.Application.Purchases.Dtos;

public class PurchaseDto
{
    public Guid Id { get; set; }

    public Guid SupplierId { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid ReceivingId { get; set; }

    public DateTime PurchaseDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal OutstandingAmount { get; set; }

    public string? Notes { get; set; }
}

public class RecordPaymentDto
{
    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}
