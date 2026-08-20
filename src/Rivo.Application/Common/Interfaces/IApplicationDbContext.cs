using Microsoft.EntityFrameworkCore;
using Rivo.Domain.Entities.Accounts;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.Expenses;
using Rivo.Domain.Entities.Notifications;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Entities.PurchaseOrders;
using Rivo.Domain.Entities.Returns;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Entities.Purchases;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Entities.Suppliers;
using Rivo.Domain.Entities.Transfers;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Entities.Warehouses;
using BarcodeEntity = Rivo.Domain.Entities.Barcodes.Barcode;
using IncomeEntity = Rivo.Domain.Entities.Income.Income;
using InventoryEntity = Rivo.Domain.Entities.Inventories.Inventory;
using InventoryItemEntity = Rivo.Domain.Entities.InventoryItems.InventoryItem;
using ReceivingEntity = Rivo.Domain.Entities.Receiving.Receiving;
using ReceivingItemEntity = Rivo.Domain.Entities.Receiving.ReceivingItem;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Application.Common.Interfaces;

/// <summary>
/// Unit-of-work seam so Application services can commit changes without depending on EF Core directly.
/// Dev1's modules go through dedicated repositories instead of this interface's DbSets; Dev2 (Inventory
/// & Operations) queries these DbSet properties directly from its services.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Written automatically by AuditSaveChangesInterceptor; also usable directly (see Dev2's IAuditService).</summary>
    DbSet<AuditLog> AuditLogs { get; }

    /// <summary>Read-only lookups for Dev2/Dev3 cross-module integration and reporting.</summary>
    DbSet<Branch> Branches { get; }

    DbSet<Product> Products { get; }

    DbSet<Order> Orders { get; }

    DbSet<OrderItem> OrderItems { get; }

    DbSet<Return> Returns { get; }

    DbSet<ReturnItem> ReturnItems { get; }

    DbSet<User> Users { get; }

    // Dev2 — Inventory & Operations
    DbSet<Warehouse> Warehouses { get; }

    DbSet<StockEntity> Stocks { get; }

    DbSet<StockMovement> StockMovements { get; }

    DbSet<Supplier> Suppliers { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

    DbSet<ReceivingEntity> Receivings { get; }

    DbSet<ReceivingItemEntity> ReceivingItems { get; }

    DbSet<Purchase> Purchases { get; }

    DbSet<Transfer> Transfers { get; }

    DbSet<TransferItem> TransferItems { get; }

    DbSet<BarcodeEntity> Barcodes { get; }

    DbSet<InventoryEntity> Inventories { get; }

    DbSet<InventoryItemEntity> InventoryItems { get; }

    // Dev3 — Finance & Intelligence
    DbSet<Account> Accounts { get; }

    DbSet<AccountTransaction> AccountTransactions { get; }

    DbSet<IncomeEntity> Incomes { get; }

    DbSet<Expense> Expenses { get; }

    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
