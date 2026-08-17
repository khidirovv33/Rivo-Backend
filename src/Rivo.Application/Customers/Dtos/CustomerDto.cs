namespace Rivo.Application.Customers.Dtos;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal TotalPurchasesAmount { get; set; }
    public int TotalOrdersCount { get; set; }
    public int LoyaltyPoints { get; set; }
}

public class CreateCustomerRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }
}

public class UpdateCustomerRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }
}
