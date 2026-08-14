# Rivo — Архитектура решения (скелет, без реализации)

Документ описывает структуру, созданную по `Rivo_TZ_Full_Team_3.md`.
Все `.cs`, `.csproj`, `.sln`, `appsettings.json`, `Program.cs` — **пустые файлы**.
Это карта проекта для ревью тимлидом/командой, реализация не начата.

## 1. Слои (Clean Architecture)

```
Rivo-Backend/
├── Rivo.sln
├── .editorconfig, .gitignore
├── src/
│   ├── Rivo.Domain/          — сущности, enum'ы, базовые контракты, доменные исключения
│   ├── Rivo.Application/     — DTO, интерфейсы сервисов, сервисы, валидаторы (по модулям)
│   ├── Rivo.Infrastructure/  — EF Core (DbContext, конфигурации, репозитории), JWT, мультитенантность, внешние сервисы, логирование
│   ├── Rivo.API/             — контроллеры, middleware, фильтры, Program.cs, appsettings
│   └── Rivo.Web/             — назначение НЕ уточнено в ТЗ, см. раздел 5 "Открытые вопросы"
├── tests/
│   ├── Rivo.UnitTests/
│   └── Rivo.IntegrationTests/
└── docs/
    ├── ERD.md, API.md, PHASES.md   — заготовки, заполнять по ходу проектирования БД/контрактов
```

Зависимости слоёв (снаружи внутрь): `API → Application → Domain`, `Infrastructure → Application/Domain`. Domain ничего не знает о верхних слоях.

## 2. Модули и владельцы (по разделу 22 ТЗ)

Каждый модуль — одинаковый набор папок в каждом слое: `Domain/Entities/<Module>`, `Application/<Module>/{Dtos,Interfaces,Services,Validators}`, `Infrastructure/Persistence/{Configurations,Repositories}/<Module>`, контроллер в `API/Controllers`.

### Developer 1 — Core & Commerce
Auth, Users, Roles, Permissions, Stores, Products, Categories, Brands, Customers, Loyalty, Pos, Orders, Payments, Returns

### Developer 2 — Inventory & Operations
Warehouses, Stock, StockMovements, Suppliers, Purchases, PurchaseOrders, Receiving, Transfers, Barcodes, Inventories, InventoryItems

### Developer 3 — Finance & Intelligence
Finance, Income, Expenses, Accounts, Analytics, Reports, Notifications, Audit, Dashboard

### Общее / инфраструктурное (не привязано к одному разработчику)
Settings, Tenancy (мульти-тенант + подписки), всё в `*/Common/*`, `Infrastructure/{Identity,Multitenancy,ExternalServices,Logging}`, `API/{Middlewares,Filters,Extensions}`

## 3. Domain/Common — сквозные контракты

- `BaseEntity` — Id, CreatedAt, UpdatedAt
- `IAuditableEntity` — для Audit Log (Who/What/When/OldValue/NewValue)
- `ITenantEntity` — TenantId, для изоляции данных компаний
- `ISoftDelete` — мягкое удаление

## 4. Тесты

Сгруппированы по зонам ответственности (`CoreCommerce`, `InventoryOperations`, `FinanceIntelligence`, `Common`), а не по модулям 1:1 — под критерий готовности "основные бизнес-сценарии покрыты тестами" (раздел 27 ТЗ).

## 5. Открытые вопросы — нужно решить перед началом кода

1. **Rivo.Web** — в разделе 3 ТЗ упомянут отдельно от Rivo.API, но назначение не расписано. Варианты:
   - это ASP.NET-хост (`Program.cs`, DI) отдельно от `Rivo.API` как библиотеки контроллеров;
   - это Blazor-фронтенд в том же solution (если выбираете Blazor вместо React);
   - React выносится в отдельный репозиторий, и Rivo.Web не нужен вовсе.
   Сейчас создан как заглушка — решите с командой и я поправлю структуру.
2. **Settings и Tenancy/Subscription** — в scope (разделы 17–18) есть, но в списке `/api/...` (раздел 20) эндпоинты не перечислены. Добавил модули и контроллеры (`SettingsController`, `TenantsController`) как естественное продолжение ТЗ — подтвердите или скорректируйте.
3. **Purchases vs PurchaseOrders** — в ТЗ фигурируют оба термина (раздел 9 и раздел 20 отдельными эндпоинтами). Сделал раздельными модулями — уточните, не дубль ли это.
4. **CQRS/MediatR** — ТЗ не упоминает, поэтому слой Application сделан в виде классических Service-классов (не Commands/Queries). Если команда хочет MediatR-подход — структура `Application/<Module>` поменяется (добавятся `Commands/`, `Queries/`).

## 6. Дальше (Phase 1 по разделу 25 ТЗ)

1. Подтвердить open-вопросы выше.
2. Наполнить `.csproj`/`Rivo.sln` (`dotnet new classlib/webapi`, ссылки между проектами).
3. Спроектировать ERD и зафиксировать в `docs/ERD.md`.
4. Наполнить `Domain/Entities` полями и связями, включая `TenantId`/`StoreId` во всех сущностях, привязанных к магазину.
5. Настроить `ApplicationDbContext`, первую миграцию, Swagger, базовую авторизацию (JWT) — это и есть MVP-фундамент Phase 1.
