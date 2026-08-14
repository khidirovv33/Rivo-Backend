using Rivo.Domain.Enums;

namespace Rivo.Application.Stores.Dtos;

public class StoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public StoreStatus Status { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal DefaultTaxRate { get; set; }
    public string? OpeningHours { get; set; }
    public List<BranchDto> Branches { get; set; } = new();
}

public class CreateStoreRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal DefaultTaxRate { get; set; }
    public string? OpeningHours { get; set; }
}

public class UpdateStoreRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public StoreStatus Status { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal DefaultTaxRate { get; set; }
    public string? OpeningHours { get; set; }
}
