using Rivo.Domain.Enums;

namespace Rivo.Application.Payments.Dtos;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaidAt { get; set; }
}

public class CreatePaymentRequestDto
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
}
