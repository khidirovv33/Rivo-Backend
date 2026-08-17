namespace Rivo.Application.Loyalty.Dtos;

public class LoyaltyLevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinimumSpend { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class CreateLoyaltyLevelRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MinimumSpend { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class UpdateLoyaltyLevelRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MinimumSpend { get; set; }
    public decimal DiscountPercentage { get; set; }
}
