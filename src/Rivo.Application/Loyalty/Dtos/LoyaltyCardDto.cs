namespace Rivo.Application.Loyalty.Dtos;

public class LoyaltyCardDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public Guid? LoyaltyLevelId { get; set; }
    public string? LoyaltyLevelName { get; set; }
    public decimal LoyaltyLevelDiscountPercentage { get; set; }
    public DateTime IssuedAt { get; set; }
    public bool IsActive { get; set; }
}

public class IssueLoyaltyCardRequestDto
{
    public Guid CustomerId { get; set; }
    public Guid? LoyaltyLevelId { get; set; }
}
