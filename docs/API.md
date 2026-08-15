# Rivo — API

Заполняется по мере реализации. Ниже — эндпоинты **Dev2 (Inventory & Operations)**.

Общий конверт ответа для всей команды: `Rivo.Application.Common.Models.ApiResponse<T>`
(`{ success, data, message, errors }`). Пагинация — `PagedRequest` (`page`, `pageSize`,
`search`, `sortBy`, `sortDescending`) в query string, ответ — `PaginatedList<T>`.
Авторизация — JWT Bearer + `[PermissionAuthorize("<Permission>")]` на каждом эндпоинте.

## Warehouses — `/api/warehouses`
- `GET /` — список (paged)
- `GET /{id}`
- `POST /` — `CreateWarehouseDto`
- `PUT /{id}` — `UpdateWarehouseDto`
- `DELETE /{id}` — soft delete

## Stock — `/api/stock`
- `GET /?warehouseId=&productId=`
- `GET /{warehouseId}/{productId}?productVariationId=`
- `POST /reserve` — `ReserveStockDto` (увеличивает Reserved)
- `POST /release-reservation` — `ReserveStockDto` (уменьшает Reserved)

## Stock Movements — `/api/stock-movements`
Единственная точка изменения `Stock.SystemQuantity` во всей системе.
- `GET /?warehouseId=&productId=`
- `GET /{id}`
- `POST /` — `CreateStockMovementDto` (`Type` — приход/расход/продажа/возврат/списание/
  корректировка/трансфер; `Quantity` — знаковая дельта)

## Suppliers — `/api/suppliers`
- `GET /`, `GET /{id}` (включает `OutstandingDebt`), `POST /`, `PUT /{id}`, `DELETE /{id}`

## Purchase Orders — `/api/purchase-orders`
- `GET /?supplierId=`, `GET /{id}`, `POST /` — `CreatePurchaseOrderDto` (с позициями)
- `POST /{id}/send`, `/confirm`, `/cancel` — переходы статуса

## Receiving — `/api/receiving`
- `GET /?purchaseOrderId=`, `GET /{id}`
- `POST /` — `CreateReceivingDto`: проводит приём (полный/частичный), пишет
  StockMovement(Receipt) на каждую позицию, создаёт `Purchase`

## Purchases — `/api/purchases`
- `GET /?supplierId=`, `GET /{id}`
- `POST /{id}/payments` — `RecordPaymentDto` (уменьшает задолженность)

## Transfers — `/api/transfers`
- `GET /?warehouseId=`, `GET /{id}`, `POST /` — `CreateTransferDto`
- `POST /{id}/submit`, `/approve` (permission `Inventory.Approve`), `/ship` (списывает
  источник), `/receive` (зачисляет получателя), `/cancel`

## Barcodes — `/api/barcodes`
- `GET /product/{productId}`, `GET /scan/{code}`
- `POST /generate` — авто EAN-13, `POST /register` — существующий код
- `DELETE /{id}`, `GET /{id}/label` — PNG для этикетки

## Inventories (Ревизия) — `/api/inventories`
- `GET /?warehouseId=`, `GET /{id}` (включает `ShortageQuantity/Cost`, `SurplusQuantity/Cost`)
- `POST /` — `CreateInventoryDto`
- `POST /{id}/complete` — фиксирует подсчёт
- `POST /{id}/approve` (permission `Inventory.Approve`) — создаёт корректирующие
  StockMovement(Adjustment) по каждой позиции с разницей
- `POST /{id}/cancel`

## Inventory Items — `/api/inventories/{inventoryId}/items`
- `GET /` — позиции ревизии
- `POST /scan` — `ScanInventoryItemDto` (upsert по товару — повторный скан обновляет факт)
- `DELETE /{itemId}`

### Inventory Adjustments

Отдельного модуля/роута нет: разовая корректировка вне ревизии — это
`POST /api/stock-movements` с `Type = Adjustment` (см. выше), контракт общий.

## Permissions, используемые Dev2-эндпоинтами
`Inventory.Read`, `Inventory.Create`, `Inventory.Approve` (раздел 4 ТЗ). Выдаются в JWT
как claim `"permission"` (контракт согласован с Dev1/Auth) — модуль-эмитент авторизации.
