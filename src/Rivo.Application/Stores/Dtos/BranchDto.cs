using Rivo.Domain.Enums;

namespace Rivo.Application.Stores.Dtos;

public class BranchDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public StoreStatus Status { get; set; }
}

public class CreateBranchRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class UpdateBranchRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public StoreStatus Status { get; set; }
}
