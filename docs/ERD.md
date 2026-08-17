# Rivo — ERD

Наполняется по мере реализации модулей каждым разработчиком. Ниже — сущности
**Dev2 (Inventory & Operations)**, реализованные в `feature`-работе на ветке `abbos`.

## Dev2 — Inventory & Operations

```
Warehouse (Guid StoreId -> Dev1.Store, пока без навигации)
 └─< Stock (WarehouseId, ProductId -> Dev1.Product, ProductVariationId?, SystemQuantity, ReservedQuantity)
 └─< StockMovement (WarehouseId, ProductId, Type, Quantity[signed], QuantityBefore/After, ReferenceType/Id)

Supplier
 └─< PurchaseOrder (SupplierId, WarehouseId, Status, OrderNumber)
       └─< PurchaseOrderItem (ProductId, Quantity, UnitCost, ReceivedQuantity)
 └─< Purchase (SupplierId, PurchaseOrderId, ReceivingId, TotalAmount, PaidAmount)

Receiving (PurchaseOrderId, WarehouseId, Status)
 └─< ReceivingItem (PurchaseOrderItemId, ProductId, QuantityReceived, UnitCost)

Transfer (SourceWarehouseId, DestinationWarehouseId, Status)
 └─< TransferItem (ProductId, Quantity)

Barcode (ProductId, ProductVariationId?, Code, Type, IsPrimary)

Inventory (WarehouseId, ResponsibleUserId, Status)
 └─< InventoryItem (ProductId, SystemQuantity[snapshot], ActualQuantity, UnitCost[snapshot])

AuditLog (владелец: Dev3 — минимальный write-контракт заведён в Phase A,
          т.к. StockMovements/PurchaseOrders/Transfers/Inventories пишут сюда по DoD)
```

### Ключевой инвариант

`Stock.SystemQuantity` меняется **только** через `IStockMovementsService.CreateAsync`
(единственная точка записи). Receiving, Transfers (ship/receive) и Inventory (approve)
— единственные вызывающие стороны внутри Dev2; в будущем Dev1 (Sales/Returns) подключится
через тот же контракт.

### Tenant isolation

Все сущности выше реализуют `ITenantEntity`. Изоляция обеспечивается глобальным EF Core
query filter (`ApplicationDbContext.ApplyTenantQueryFilter`, именованный фильтр
`"TenantIsolation"`), комбинируемым AND'ом с любым другим фильтром на той же сущности
(например `"SoftDelete"` на Warehouse/Supplier).

## Открытые связи с другими модулями (FK без навигации, ждут Dev1)

- `Warehouse.StoreId` → `Store` (Dev1)
- `Stock.ProductId`, `PurchaseOrderItem.ProductId`, `TransferItem.ProductId`,
  `Barcode.ProductId`, `InventoryItem.ProductId` → `Product` (Dev1)
- `*.ProductVariationId` → `ProductVariation` (Dev1)

Когда Dev1 реализует эти сущности, натуральный следующий шаг — добавить навигационные
свойства и настоящие FK-констрейнты в соответствующих `*Configuration.cs`.
