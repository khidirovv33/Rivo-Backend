using Rivo.Domain.Common;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Domain.Entities.Warehouses;

public class Warehouse : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    /// <summary>FK -> Store (модуль Dev1). Навигационное свойство не подключено.</summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// FK -> Branch (модуль Dev1), опционально. Используется StockAdjustmentService для определения
    /// "какой склад списывать/пополнять" при продаже/возврате на конкретном филиале — см.
    /// Application/StockMovements/Services/StockAdjustmentService.cs.
    /// </summary>
    public Guid? BranchId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<StockEntity> Stocks { get; set; } = new List<StockEntity>();
}
