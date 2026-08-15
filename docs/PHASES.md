# Rivo — Этапы разработки

По разделу 25 ТЗ. Статус ниже отражает то, что сделано на ветке `abbos` (Dev2).

| Phase | Название | Dev2-часть | Статус (Dev2) |
|---|---|---|---|
| 1 | Foundation | Общая заготовка solution/EF/Auth-каркаса (взято на себя, т.к. блокировало любой код) | ✅ |
| 2 | Store | Stores, Products, Categories, Brands (Dev1) | — |
| 3 | Warehouse | Stock, Warehouse, Stock Movements, Suppliers, Purchases | ✅ |
| 4 | Sales | POS, Cart, Orders, Payments, Returns (Dev1) | — |
| 5 | Inventory | Revision, Barcode, Difference, Stock Adjustment | ✅ |
| 6 | Finance | Income, Expenses, Accounts, Profit (Dev3) | — |
| 7 | Dashboard & Analytics | (Dev3) | — |
| 8 | Business | Customers, Loyalty, Notifications, Audit Logs (Dev1/Dev3; AuditLog write-контракт заведён в Phase 1) | частично |
| 9 | SaaS | Multi-Tenant, Subscription (общее; tenant isolation для Dev2-модулей реализована) | частично |
| 10 | Future | Mobile, AI | — |

## Dev2 (Inventory & Operations) — детальная разбивка

1. Foundation — solution/csproj/EF/JWT/Swagger каркас
2. Warehouse / Stock / StockMovements — единая точка изменения остатка
3. Suppliers / PurchaseOrders / Purchases / Receiving
4. Transfers
5. Barcode (генерация EAN-13, скан, PNG-этикетка)
6. Inventory / Revision + Inventory Adjustments
7. Hardening — FluentValidation подключена в pipeline (`ValidationFilter`), интеграционные
   тесты трёх ключевых сценариев из DoD (закупка увеличивает остаток, перемещение двигает
   остаток между складами, утверждение ревизии корректирует остаток), докстринг API/ERD

См. `docs/API.md`, `docs/ERD.md` за деталями по каждому модулю.
