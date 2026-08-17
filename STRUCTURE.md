# Rivo — Шпаргалка по структуре проекта

Как быстро найти нужный файл. Общая логика: у каждого из 34 модулей во всех слоях
папка называется одинаково (`<Module>`), внутри — одинаковый набор файлов. Зная
паттерн из раздела 2, легко найти что угодно даже без таблицы.

## 1. Верхний уровень

```
Rivo-Backend/
├── ARCHITECTURE.md   — почему так спроектировано, зоны ответственности, открытые вопросы
├── STRUCTURE.md       — этот файл: где что лежит
├── Rivo.sln           — solution, объединяет 5 проектов
├── docs/              — ERD.md, API.md, PHASES.md (заготовки, заполняются по ходу)
├── src/
│   ├── Rivo.Domain/           — сущности, enum'ы, базовые контракты
│   ├── Rivo.Application/      — DTO, интерфейсы сервисов, сервисы (по модулям)
│   ├── Rivo.Infrastructure/   — EF Core, JWT, мультитенант, внешние сервисы
│   ├── Rivo.API/              — контроллеры, middleware, Program.cs
│   └── Rivo.Web/              — назначение пока не подтверждено (см. ARCHITECTURE.md §5)
└── tests/
    ├── Rivo.UnitTests/
    └── Rivo.IntegrationTests/
```

## 2. Единый паттерн модуля (пример — `Products`)

Любой из 34 модулей раскладывается по слоям вот так:

```
src/Rivo.Domain/Entities/Products/
├── Product.cs
└── ProductVariation.cs

src/Rivo.Application/Products/
├── Dtos/
│   ├── ProductDto.cs
│   └── ProductVariationDto.cs
├── Interfaces/
│   └── IProductsService.cs
├── Services/
│   └── ProductsService.cs
└── Validators/            (пусто, .gitkeep — FluentValidation-правила добавятся позже)

src/Rivo.Infrastructure/Persistence/Configurations/Products/
├── ProductConfiguration.cs
└── ProductVariationConfiguration.cs

src/Rivo.Infrastructure/Persistence/Repositories/Products/
├── IProductsRepository.cs
└── ProductsRepository.cs

src/Rivo.API/Controllers/
└── ProductsController.cs
```

Т.е. чтобы найти что-либо по модулю `X` — сущность в `Domain/Entities/X/`,
DTO/сервис в `Application/X/`, EF-конфиг/репозиторий в
`Infrastructure/Persistence/{Configurations,Repositories}/X/`, контроллер —
`API/Controllers/XController.cs`.

## 3. Таблица всех 34 модулей

| Модуль (папка) | Владелец (ТЗ §22) | Domain-сущности | API-контроллер |
|---|---|---|---|
| Auth | Dev1 | RefreshToken | AuthController |
| Users | Dev1 | User | UsersController |
| Roles | Dev1 | Role | RolesController |
| Permissions | Dev1 | Permission, RolePermission | PermissionsController |
| Stores | Dev1 | Store, Branch | StoresController |
| Products | Dev1 | Product, ProductVariation | ProductsController |
| Categories | Dev1 | Category | CategoriesController |
| Brands | Dev1 | Brand | BrandsController |
| Customers | Dev1 | Customer | CustomersController |
| Loyalty | Dev1 | LoyaltyCard, LoyaltyLevel | LoyaltyController |
| Pos | Dev1 | — (оркестрирует Sales) | PosController |
| Orders | Dev1 | Order, OrderItem | OrdersController |
| Payments | Dev1 | Payment | PaymentsController |
| Returns | Dev1 | Return, ReturnItem | ReturnsController |
| Warehouses | Dev2 | Warehouse | WarehousesController |
| Stock | Dev2 | Stock | StockController |
| StockMovements | Dev2 | StockMovement | StockMovementsController |
| Suppliers | Dev2 | Supplier | SuppliersController |
| Purchases | Dev2 | Purchase | PurchasesController |
| PurchaseOrders | Dev2 | PurchaseOrder, PurchaseOrderItem | PurchaseOrdersController |
| Receiving | Dev2 | Receiving, ReceivingItem | ReceivingController |
| Transfers | Dev2 | Transfer, TransferItem | TransfersController |
| Inventories | Dev2 | Inventory | InventoriesController |
| InventoryItems | Dev2 | InventoryItem | InventoryItemsController |
| Barcodes | Dev2 | Barcode | BarcodesController |
| Finance | Dev3 | — (агрегирует Income/Expenses) | FinanceController |
| Income | Dev3 | Income | IncomeController |
| Expenses | Dev3 | Expense | ExpensesController |
| Accounts | Dev3 | Account, AccountTransaction | AccountsController |
| Analytics | Dev3 | — (read-model) | AnalyticsController |
| Reports | Dev3 | — (генерируется) | ReportsController |
| Notifications | Dev3 | Notification | NotificationsController |
| Audit | Dev3 | AuditLog | AuditController |
| Dashboard | Dev3 | — (агрегирует данные) | DashboardController |
| Settings *(допущение)* | общее | StoreSettings | SettingsController |
| Tenancy *(допущение)* | общее | Tenant, Subscription, SubscriptionPlan | TenantsController |

Полный путь по любой строке: `src/Rivo.Domain/Entities/<Модуль>/`,
`src/Rivo.Application/<Модуль>/`, `src/Rivo.Infrastructure/Persistence/*/<Модуль>/`,
`src/Rivo.API/Controllers/<Контроллер>.cs`.

## 4. Сквозные вещи — не привязаны к одному модулю

| Что | Где |
|---|---|
| Базовые контракты сущностей (BaseEntity, ITenantEntity, ISoftDelete, IAuditableEntity) | `src/Rivo.Domain/Common/` |
| Общие enum'ы (OrderStatus, PaymentMethod, TransferStatus...) | `src/Rivo.Domain/Enums/` |
| Доменные исключения (NotFoundException и т.д.) | `src/Rivo.Domain/Exceptions/` |
| Общие интерфейсы приложения (ICurrentUserService, IApplicationDbContext...) | `src/Rivo.Application/Common/Interfaces/` |
| AutoMapper/Mapster профиль | `src/Rivo.Application/Common/Mappings/MappingProfile.cs` |
| Пагинация, ApiResponse | `src/Rivo.Application/Common/Models/` |
| EF Core DbContext | `src/Rivo.Infrastructure/Persistence/ApplicationDbContext.cs` |
| Миграции EF | `src/Rivo.Infrastructure/Persistence/Migrations/` |
| JWT / хэширование паролей | `src/Rivo.Infrastructure/Identity/` |
| Мультитенантность (resolve tenant) | `src/Rivo.Infrastructure/Multitenancy/` |
| Email, PDF/Excel/CSV экспорт, файлы, генерация штрихкодов | `src/Rivo.Infrastructure/ExternalServices/` |
| Serilog | `src/Rivo.Infrastructure/Logging/` |
| Обработка ошибок, аудит-логирование, tenant middleware | `src/Rivo.API/Middlewares/` |
| Permission-based авторизация (атрибут) | `src/Rivo.API/Filters/PermissionAuthorizeAttribute.cs` |
| Регистрация DI, Swagger | `src/Rivo.API/Extensions/` |
| Точка входа API | `src/Rivo.API/Program.cs`, `appsettings.json` |

## 5. Тесты

Сгруппированы не по модулям, а по зонам ответственности (ТЗ §22), чтобы дев видел свою папку целиком:

```
tests/Rivo.UnitTests/{Common, CoreCommerce, InventoryOperations, FinanceIntelligence}/
tests/Rivo.IntegrationTests/{Common, CoreCommerce, InventoryOperations, FinanceIntelligence}/
```
`CoreCommerce` = зона Dev1, `InventoryOperations` = зона Dev2, `FinanceIntelligence` = зона Dev3.

## 6. Примеры "куда полезть"

- Нужно добавить поле в товар → `src/Rivo.Domain/Entities/Products/Product.cs`
- Нужен DTO для возврата → `src/Rivo.Application/Returns/Dtos/ReturnDto.cs`
- Нужна EF-конфигурация склада → `src/Rivo.Infrastructure/Persistence/Configurations/Warehouses/WarehouseConfiguration.cs`
- Нужен новый эндпоинт для ревизии → `src/Rivo.API/Controllers/InventoriesController.cs`
- Нужно поменять логику JWT → `src/Rivo.Infrastructure/Identity/JwtTokenService.cs`
- Нужно поправить middleware аудит-лога → `src/Rivo.API/Middlewares/AuditLoggingMiddleware.cs`
