using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rivo.Application.Accounts.Dtos;
using Rivo.Application.Accounts.Interfaces;
using Rivo.Application.Auth.Dtos;
using Rivo.Application.Auth.Interfaces;
using Rivo.Application.Brands.Dtos;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Categories.Dtos;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Customers.Dtos;
using Rivo.Application.Customers.Interfaces;
using Rivo.Application.Expenses.Dtos;
using Rivo.Application.Expenses.Interfaces;
using Rivo.Application.Inventories.Dtos;
using Rivo.Application.Inventories.Interfaces;
using Rivo.Application.InventoryItems.Dtos;
using Rivo.Application.InventoryItems.Interfaces;
using Rivo.Application.Loyalty.Dtos;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Payments.Dtos;
using Rivo.Application.Pos.Dtos;
using Rivo.Application.Pos.Interfaces;
using Rivo.Application.Products.Dtos;
using Rivo.Application.Products.Interfaces;
using Rivo.Application.PurchaseOrders.Dtos;
using Rivo.Application.PurchaseOrders.Interfaces;
using Rivo.Application.Purchases.Dtos;
using Rivo.Application.Purchases.Interfaces;
using Rivo.Application.Receiving.Dtos;
using Rivo.Application.Receiving.Interfaces;
using Rivo.Application.Returns.Dtos;
using Rivo.Application.Returns.Interfaces;
using Rivo.Application.Stores.Dtos;
using Rivo.Application.Stores.Interfaces;
using Rivo.Application.Suppliers.Dtos;
using Rivo.Application.Suppliers.Interfaces;
using Rivo.Application.Transfers.Dtos;
using Rivo.Application.Transfers.Interfaces;
using Rivo.Application.Warehouses.Dtos;
using Rivo.Application.Warehouses.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Infrastructure.Persistence.Seed;

/// <summary>
/// Demo tenant with realistic data across every module (Core & Commerce / Inventory & Operations /
/// Finance & Intelligence) so a fresh checkout shows a populated app instead of empty states. Runs once
/// (Development only, see Program.cs) — idempotent on the demo account's email, isolated in its own
/// tenant so it never touches or conflicts with any other tenant already in the database.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoEmail = "demo@rivo.uz";
    public const string DemoPassword = "Demo12345!";

    public static async Task SeedAsync(IServiceProvider rootProvider, CancellationToken cancellationToken = default)
    {
        using var scope = rootProvider.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();

        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == DemoEmail, cancellationToken))
        {
            return;
        }

        var auth = await services.GetRequiredService<IAuthService>().RegisterAsync(new RegisterRequestDto
        {
            CompanyName = "Rivo Demo",
            FullName = "Демо Владелец",
            Email = DemoEmail,
            Password = DemoPassword,
            PhoneNumber = "+998901234567",
        }, cancellationToken);

        var tenantId = auth.TenantId;
        var userId = auth.UserId;

        // Dev2/Dev3 services read tenant/user from the ambient HTTP context (ICurrentTenantService /
        // ICurrentUserService), which doesn't exist during startup seeding. Fake it once, matching the
        // exact claim shape JwtTokenService issues, so every service call below behaves as if it came
        // through a real authenticated request.
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, DemoEmail),
            new("tenant_id", tenantId.ToString()),
            new("role_id", Guid.Empty.ToString()),
            new(ClaimTypes.Role, auth.RoleName),
        };
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Seed")),
            RequestServices = services,
        };

        var storesService = services.GetRequiredService<IStoresService>();
        var categoriesService = services.GetRequiredService<ICategoriesService>();
        var brandsService = services.GetRequiredService<IBrandsService>();
        var productsService = services.GetRequiredService<IProductsService>();
        var customersService = services.GetRequiredService<ICustomersService>();
        var loyaltyService = services.GetRequiredService<ILoyaltyService>();
        var warehousesService = services.GetRequiredService<IWarehousesService>();
        var suppliersService = services.GetRequiredService<ISuppliersService>();
        var purchaseOrdersService = services.GetRequiredService<IPurchaseOrdersService>();
        var receivingService = services.GetRequiredService<IReceivingService>();
        var purchasesService = services.GetRequiredService<IPurchasesService>();
        var transfersService = services.GetRequiredService<ITransfersService>();
        var inventoriesService = services.GetRequiredService<IInventoriesService>();
        var inventoryItemsService = services.GetRequiredService<IInventoryItemsService>();
        var posService = services.GetRequiredService<IPosService>();
        var returnsService = services.GetRequiredService<IReturnsService>();
        var accountsService = services.GetRequiredService<IAccountsService>();
        var expensesService = services.GetRequiredService<IExpensesService>();

        var store = await storesService.CreateAsync(tenantId, new CreateStoreRequestDto
        {
            Name = "Rivo Market",
            Address = "г. Ташкент, ул. Амира Темура, 15",
            Phone = "+998712001122",
            Email = "market@rivo.uz",
            Currency = "UZS",
            DefaultTaxRate = 12,
            OpeningHours = "09:00–21:00",
        }, cancellationToken);

        var branch = await storesService.AddBranchAsync(tenantId, store.Id, new CreateBranchRequestDto
        {
            Name = "Чиланзарский филиал",
            Address = "г. Ташкент, Чиланзарский р-н, 12 квартал",
            Phone = "+998712003344",
        }, cancellationToken);

        var categories = new List<CategoryDto>();
        foreach (var name in new[] { "Напитки", "Снеки", "Молочные продукты", "Бытовая химия" })
        {
            categories.Add(await categoriesService.CreateAsync(tenantId, new CreateCategoryRequestDto { Name = name }, cancellationToken));
        }

        var brands = new List<BrandDto>();
        foreach (var name in new[] { "Coca-Cola", "Nestle", "President", "Local Fresh" })
        {
            brands.Add(await brandsService.CreateAsync(tenantId, new CreateBrandRequestDto { Name = name }, cancellationToken));
        }

        var productSeeds = new (string Name, string Sku, int CategoryIdx, int BrandIdx, decimal Purchase, decimal Sell, int MinStock)[]
        {
            ("Coca-Cola 1.5л", "BEV-001", 0, 0, 6500m, 9500m, 20),
            ("Coca-Cola Zero 0.5л", "BEV-002", 0, 0, 3500m, 5500m, 30),
            ("Нескафе Классик 100г", "BEV-003", 0, 1, 22000m, 29900m, 10),
            ("Lays Классический 150г", "SNK-001", 1, 3, 8500m, 12900m, 25),
            ("Читос Кетчуп 85г", "SNK-002", 1, 3, 5200m, 7900m, 25),
            ("Молоко Президент 1л", "DAI-001", 2, 2, 9800m, 13500m, 15),
            ("Йогурт Президент 500г", "DAI-002", 2, 2, 11200m, 15900m, 15),
            ("Фейри 450мл", "CHM-001", 3, 3, 12500m, 17900m, 10),
        };

        var products = new List<ProductDto>();
        foreach (var p in productSeeds)
        {
            products.Add(await productsService.CreateAsync(tenantId, new CreateProductRequestDto
            {
                Name = p.Name,
                Sku = p.Sku,
                CategoryId = categories[p.CategoryIdx].Id,
                BrandId = brands[p.BrandIdx].Id,
                PurchasePrice = p.Purchase,
                SellingPrice = p.Sell,
                Unit = "шт",
                MinimumStock = p.MinStock,
                TaxRate = 12,
            }, cancellationToken));
        }

        var bronze = await loyaltyService.CreateLevelAsync(tenantId, new CreateLoyaltyLevelRequestDto { Name = "Бронза", MinimumSpend = 0, DiscountPercentage = 0 }, cancellationToken);
        var silver = await loyaltyService.CreateLevelAsync(tenantId, new CreateLoyaltyLevelRequestDto { Name = "Серебро", MinimumSpend = 500000, DiscountPercentage = 5 }, cancellationToken);
        await loyaltyService.CreateLevelAsync(tenantId, new CreateLoyaltyLevelRequestDto { Name = "Золото", MinimumSpend = 2000000, DiscountPercentage = 10 }, cancellationToken);

        var customers = new List<CustomerDto>();
        foreach (var c in new (string Name, string Phone)[]
                 {
                     ("Азиз Каримов", "+998901112233"),
                     ("Дилноза Юсупова", "+998933334455"),
                     ("Санжар Абдуллаев", "+998971234567"),
                 })
        {
            customers.Add(await customersService.CreateAsync(tenantId, new CreateCustomerRequestDto { FullName = c.Name, Phone = c.Phone }, cancellationToken));
        }

        // Cards exist so the Loyalty screens aren't empty, but no seeded sale uses these customers —
        // that keeps the checkout total math below exact without having to replicate tier-discount rounding.
        await loyaltyService.IssueCardAsync(tenantId, new IssueLoyaltyCardRequestDto { CustomerId = customers[0].Id, LoyaltyLevelId = silver.Id }, cancellationToken);
        await loyaltyService.IssueCardAsync(tenantId, new IssueLoyaltyCardRequestDto { CustomerId = customers[1].Id, LoyaltyLevelId = bronze.Id }, cancellationToken);

        // Main warehouse MUST be linked to this branch — StockAdjustmentService resolves "the" branch
        // warehouse by BranchId, so POS sales below decrement stock here.
        var mainWarehouse = await warehousesService.CreateAsync(new CreateWarehouseDto
        {
            StoreId = store.Id,
            BranchId = branch.Id,
            Name = "Основной склад",
            Address = branch.Address,
        }, cancellationToken);

        var secondaryWarehouse = await warehousesService.CreateAsync(new CreateWarehouseDto
        {
            StoreId = store.Id,
            Name = "Резервный склад",
        }, cancellationToken);

        var supplier1 = await suppliersService.CreateAsync(new CreateSupplierDto
        {
            Name = "ООО Ташкент Дистрибьюшн",
            ContactPerson = "Ботир Рахимов",
            Phone = "+998712220011",
            Email = "sales@tashdistrib.uz",
        }, cancellationToken);

        await suppliersService.CreateAsync(new CreateSupplierDto
        {
            Name = "ИП Молочный Дом",
            ContactPerson = "Нодира Исмоилова",
            Phone = "+998933332211",
        }, cancellationToken);

        var po = await purchaseOrdersService.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = supplier1.Id,
            WarehouseId = mainWarehouse.Id,
            ExpectedDate = DateTime.UtcNow.AddDays(2),
            Items = products.Select(p => new CreatePurchaseOrderItemDto { ProductId = p.Id, Quantity = 80, UnitCost = p.PurchasePrice }).ToList(),
        }, cancellationToken);
        await purchaseOrdersService.SendAsync(po.Id, cancellationToken);
        await purchaseOrdersService.ConfirmAsync(po.Id, cancellationToken);

        var poFull = await purchaseOrdersService.GetByIdAsync(po.Id, cancellationToken);
        await receivingService.CreateAsync(new CreateReceivingDto
        {
            PurchaseOrderId = po.Id,
            Items = poFull.Items.Select(i => new CreateReceivingItemDto { PurchaseOrderItemId = i.Id, QuantityReceived = i.Quantity }).ToList(),
        }, cancellationToken);

        var purchasesPage = await purchasesService.GetAllAsync(new PagedRequest { PageNumber = 1, PageSize = 10 }, supplier1.Id, cancellationToken);
        var purchase = purchasesPage.Items.First();
        await purchasesService.RecordPaymentAsync(purchase.Id, new RecordPaymentDto { Amount = Math.Round(purchase.TotalAmount * 0.6m, 0) }, cancellationToken);

        var transfer = await transfersService.CreateAsync(new CreateTransferDto
        {
            SourceWarehouseId = mainWarehouse.Id,
            DestinationWarehouseId = secondaryWarehouse.Id,
            Notes = "Пополнение резервного склада",
            Items = products.Take(3).Select(p => new CreateTransferItemDto { ProductId = p.Id, Quantity = 10 }).ToList(),
        }, cancellationToken);
        await transfersService.SubmitAsync(transfer.Id, cancellationToken);
        await transfersService.ApproveAsync(transfer.Id, cancellationToken);
        await transfersService.ShipAsync(transfer.Id, cancellationToken);
        await transfersService.ReceiveAsync(transfer.Id, cancellationToken);

        var inventory = await inventoriesService.CreateAsync(new CreateInventoryDto { WarehouseId = mainWarehouse.Id, Notes = "Плановая ревизия" }, cancellationToken);
        await inventoryItemsService.ScanAsync(inventory.Id, new ScanInventoryItemDto { ProductId = products[0].Id, ActualQuantity = 68 }, cancellationToken);
        await inventoryItemsService.ScanAsync(inventory.Id, new ScanInventoryItemDto { ProductId = products[3].Id, ActualQuantity = 81 }, cancellationToken);
        await inventoriesService.CompleteAsync(inventory.Id, cancellationToken);
        await inventoriesService.ApproveAsync(inventory.Id, cancellationToken);

        var cashAccount = await accountsService.CreateAsync(new CreateAccountDto { Name = "Касса", Type = AccountType.Cash }, cancellationToken);
        await accountsService.CreateAsync(new CreateAccountDto { Name = "Расчётный счёт", Type = AccountType.Bank }, cancellationToken);

        var expenseIds = new List<(Guid Id, int DaysAgo)>();
        foreach (var e in new (ExpenseCategory Category, decimal Amount, string Description, int DaysAgo)[]
                 {
                     (ExpenseCategory.Rent, 350000m, "Аренда за месяц", 5),
                     (ExpenseCategory.Salary, 550000m, "Зарплата продавцов", 3),
                     (ExpenseCategory.Utilities, 90000m, "Коммунальные услуги", 2),
                     (ExpenseCategory.Advertising, 60000m, "Реклама в соцсетях", 1),
                 })
        {
            var expense = await expensesService.CreateAsync(new CreateExpenseDto
            {
                AccountId = cashAccount.Id,
                Category = e.Category,
                Amount = e.Amount,
                Description = e.Description,
            }, cancellationToken);
            expenseIds.Add((expense.Id, e.DaysAgo));
        }

        // POS sales spread across the last week — no customer/discount, so the payment total is a
        // pure sum-of-lines-plus-tax that exactly matches what PosService.CheckoutAsync recomputes
        // server-side (it never trusts a client-sent total).
        var random = new Random(42);
        var orderIds = new List<(Guid Id, int DaysAgo)>();
        Rivo.Application.Orders.Dtos.OrderDto? returnCandidateOrder = null;

        for (var day = 6; day >= 0; day--)
        {
            var salesCount = day == 0 ? 6 : random.Next(3, 7);
            for (var i = 0; i < salesCount; i++)
            {
                var basket = products.OrderBy(_ => random.Next()).Take(random.Next(2, 5)).ToList();
                var items = basket.Select(p => new CheckoutItemRequestDto { ProductId = p.Id, Quantity = random.Next(1, 5), DiscountAmount = 0 }).ToList();

                decimal subtotal = 0, tax = 0;
                foreach (var item in items)
                {
                    var product = basket.First(p => p.Id == item.ProductId);
                    var lineSubtotal = product.SellingPrice * item.Quantity;
                    subtotal += lineSubtotal;
                    tax += Math.Round(lineSubtotal * product.TaxRate / 100m, 2);
                }
                var total = subtotal + tax;

                var order = await posService.CheckoutAsync(tenantId, userId, new CheckoutRequestDto
                {
                    StoreId = store.Id,
                    BranchId = branch.Id,
                    OrderDiscountAmount = 0,
                    Items = items,
                    Payments = new List<CreatePaymentRequestDto> { new() { Method = PaymentMethod.Cash, Amount = total } },
                }, cancellationToken);

                orderIds.Add((order.Id, day));
                if (day == 3 && returnCandidateOrder is null && order.Items.Count > 0)
                {
                    returnCandidateOrder = order;
                }
            }
        }

        if (returnCandidateOrder is not null)
        {
            var firstItem = returnCandidateOrder.Items[0];
            await returnsService.CreateAsync(tenantId, userId, new CreateReturnRequestDto
            {
                OrderId = returnCandidateOrder.Id,
                Reason = "Клиент передумал",
                Items = new List<CreateReturnItemRequestDto>
                {
                    new() { OrderItemId = firstItem.Id, Quantity = 1 },
                },
            }, cancellationToken);
        }

        // Backdate CreatedAt/IncomeDate/ExpenseDate so the weekly Dashboard chart and Finance/Profit
        // trend actually show a spread instead of a single spike on "today". The interceptor always
        // stamps CreatedAt = now on insert, so this has to happen as a follow-up raw update.
        foreach (var (orderId, daysAgo) in orderIds)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"Orders\" SET \"CreatedAt\" = {date} WHERE \"Id\" = {orderId}", cancellationToken);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"Incomes\" SET \"IncomeDate\" = {date} WHERE \"ReferenceType\" = 'Order' AND \"ReferenceId\" = {orderId}", cancellationToken);
        }

        foreach (var (expenseId, daysAgo) in expenseIds)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"Expenses\" SET \"ExpenseDate\" = {date} WHERE \"Id\" = {expenseId}", cancellationToken);
        }
    }
}
