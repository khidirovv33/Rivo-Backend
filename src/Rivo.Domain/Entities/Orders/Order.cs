using Rivo.Domain.Common;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Orders;

/// <summary>A POS sale transaction.</summary>
public class Order : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid CashierUserId { get; set; }
    public User CashierUser { get; set; } = null!;

    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Completed;

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payments.Payment> Payments { get; set; } = new List<Payments.Payment>();
}
