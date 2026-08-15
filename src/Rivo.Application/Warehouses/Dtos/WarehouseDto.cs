namespace Rivo.Application.Warehouses.Dtos;

public class WarehouseDto
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateWarehouseDto
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }
}

public class UpdateWarehouseDto
{
    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
