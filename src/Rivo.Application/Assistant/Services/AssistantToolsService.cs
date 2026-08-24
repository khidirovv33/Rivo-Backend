using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rivo.Application.Assistant.Dtos;
using Rivo.Application.Assistant.Interfaces;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Inventories.Dtos;
using Rivo.Application.Inventories.Interfaces;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Application.Products.Dtos;
using Rivo.Application.Products.Interfaces;
using Rivo.Application.Roles.Dtos;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Stores.Dtos;
using Rivo.Application.Stores.Interfaces;
using Rivo.Application.Users.Dtos;
using Rivo.Application.Users.Interfaces;
using Rivo.Application.Warehouses.Interfaces;
using Rivo.Domain.Constants;

namespace Rivo.Application.Assistant.Services;

public class AssistantToolsService : IAssistantToolsService
{
    // toolName -> право, которым он гейтится. И то, какие инструменты вообще предлагаются модели,
    // и повторная проверка непосредственно перед выполнением — на случай, если права роли изменились
    // между тем, как модель получила список инструментов, и тем, как она решила один из них вызвать.
    private static readonly Dictionary<string, string> ToolPermissions = new()
    {
        ["create_employee"] = "Users.Create",
        ["start_inventory_audit"] = "Inventory.Create",
        ["create_product"] = "Products.Create",
        ["create_role"] = "Roles.Create",
        ["create_store"] = "Stores.Create",
    };

    private static readonly AssistantToolDefinition CreateEmployeeTool = new(
        "create_employee",
        "Создаёт нового сотрудника в системе Rivo с указанной ролью. Пароль генерируется автоматически " +
        "и возвращается в ответе — сообщи его пользователю.",
        new
        {
            type = "object",
            properties = new
            {
                fullName = new { type = "string", description = "Полное имя сотрудника" },
                email = new { type = "string", description = "Email сотрудника — используется для входа в систему" },
                roleName = new { type = "string", description = "Название роли сотрудника, например Cashier, Manager, Admin" },
                phoneNumber = new { type = "string", description = "Номер телефона сотрудника (необязательно)" },
            },
            required = new[] { "fullName", "email", "roleName" },
        });

    private static readonly AssistantToolDefinition StartInventoryAuditTool = new(
        "start_inventory_audit",
        "Запускает новую ревизию (инвентаризацию) на указанном складе.",
        new
        {
            type = "object",
            properties = new
            {
                warehouseName = new { type = "string", description = "Название склада, на котором нужно провести ревизию" },
                notes = new { type = "string", description = "Комментарий к ревизии (необязательно)" },
            },
            required = new[] { "warehouseName" },
        });

    private static readonly AssistantToolDefinition CreateProductTool = new(
        "create_product",
        "Добавляет новый товар в каталог. Категорию и бренд можно указать по названию — если такие " +
        "уже существуют, товар привяжется к ним, иначе останется без категории/бренда.",
        new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string", description = "Название товара" },
                sku = new { type = "string", description = "Артикул (SKU) товара" },
                purchasePrice = new { type = "number", description = "Закупочная цена" },
                sellingPrice = new { type = "number", description = "Цена продажи" },
                categoryName = new { type = "string", description = "Название существующей категории (необязательно)" },
                brandName = new { type = "string", description = "Название существующего бренда (необязательно)" },
                barcode = new { type = "string", description = "Штрихкод товара (необязательно)" },
                unit = new { type = "string", description = "Единица измерения, например «шт», «кг» (необязательно, по умолчанию «шт»)" },
                minimumStock = new { type = "number", description = "Минимальный остаток для оповещений (необязательно, по умолчанию 0)" },
                taxRate = new { type = "number", description = "Ставка налога, % (необязательно, по умолчанию 0)" },
            },
            required = new[] { "name", "sku", "purchasePrice", "sellingPrice" },
        });

    private static readonly AssistantToolDefinition CreateRoleTool = new(
        "create_role",
        "Создаёт новую роль с указанным набором прав. Каждое право — строка вида \"Модуль.Действие\", " +
        "например \"Products.Read\", \"Sales.Create\". Полный каталог: " + string.Join(", ", PermissionNames.All()) + ".",
        new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string", description = "Название роли, например Cashier, Manager" },
                description = new { type = "string", description = "Описание роли (необязательно)" },
                permissionNames = new
                {
                    type = "array",
                    items = new { type = "string" },
                    description = "Список прав в формате \"Модуль.Действие\" из каталога выше",
                },
            },
            required = new[] { "name", "permissionNames" },
        });

    private static readonly AssistantToolDefinition CreateStoreTool = new(
        "create_store",
        "Создаёт новый магазин (точку продаж) в системе.",
        new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string", description = "Название магазина" },
                address = new { type = "string", description = "Адрес магазина (необязательно)" },
                phone = new { type = "string", description = "Телефон магазина (необязательно)" },
                email = new { type = "string", description = "Email магазина (необязательно)" },
                currency = new { type = "string", description = "Код валюты из 3 букв, например UZS (необязательно, по умолчанию UZS)" },
                defaultTaxRate = new { type = "number", description = "Ставка налога по умолчанию, % (необязательно, по умолчанию 0)" },
            },
            required = new[] { "name" },
        });

    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IUsersService _usersService;
    private readonly IRolesService _rolesService;
    private readonly IWarehousesService _warehousesService;
    private readonly IInventoriesService _inventoriesService;
    private readonly IProductsService _productsService;
    private readonly ICategoriesService _categoriesService;
    private readonly IBrandsService _brandsService;
    private readonly IStoresService _storesService;

    public AssistantToolsService(
        IPermissionsRepository permissionsRepository,
        IUsersService usersService,
        IRolesService rolesService,
        IWarehousesService warehousesService,
        IInventoriesService inventoriesService,
        IProductsService productsService,
        ICategoriesService categoriesService,
        IBrandsService brandsService,
        IStoresService storesService)
    {
        _permissionsRepository = permissionsRepository;
        _usersService = usersService;
        _rolesService = rolesService;
        _warehousesService = warehousesService;
        _inventoriesService = inventoriesService;
        _productsService = productsService;
        _categoriesService = categoriesService;
        _brandsService = brandsService;
        _storesService = storesService;
    }

    public async Task<List<AssistantToolDefinition>> GetAvailableToolsAsync(AssistantContext context, CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionsRepository.GetByRoleIdAsync(context.RoleId, cancellationToken);
        var names = permissions.Select(p => p.Name).ToHashSet();

        var tools = new List<AssistantToolDefinition>();
        if (names.Contains("Users.Create"))
        {
            tools.Add(CreateEmployeeTool);
        }

        if (names.Contains("Inventory.Create"))
        {
            tools.Add(StartInventoryAuditTool);
        }

        if (names.Contains("Products.Create"))
        {
            tools.Add(CreateProductTool);
        }

        if (names.Contains("Roles.Create"))
        {
            tools.Add(CreateRoleTool);
        }

        if (names.Contains("Stores.Create"))
        {
            tools.Add(CreateStoreTool);
        }

        return tools;
    }

    public async Task<string> ExecuteAsync(string toolName, JsonElement arguments, AssistantContext context, CancellationToken cancellationToken = default)
    {
        if (!ToolPermissions.TryGetValue(toolName, out var requiredPermission))
        {
            return Error($"Неизвестное действие: {toolName}");
        }

        var permissions = await _permissionsRepository.GetByRoleIdAsync(context.RoleId, cancellationToken);
        if (!permissions.Any(p => p.Name == requiredPermission))
        {
            return Error($"У вас нет права \"{requiredPermission}\" для этого действия.");
        }

        return toolName switch
        {
            "create_employee" => await CreateEmployeeAsync(arguments, context, cancellationToken),
            "start_inventory_audit" => await StartInventoryAuditAsync(arguments, context, cancellationToken),
            "create_product" => await CreateProductAsync(arguments, context, cancellationToken),
            "create_role" => await CreateRoleAsync(arguments, context, cancellationToken),
            "create_store" => await CreateStoreAsync(arguments, context, cancellationToken),
            _ => Error($"Неизвестное действие: {toolName}"),
        };
    }

    private async Task<string> CreateEmployeeAsync(JsonElement args, AssistantContext context, CancellationToken cancellationToken)
    {
        var fullName = GetString(args, "fullName");
        var email = GetString(args, "email");
        var roleName = GetString(args, "roleName");
        var phoneNumber = GetString(args, "phoneNumber");

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(roleName))
        {
            return Error("Не хватает данных: нужны имя, email и роль сотрудника.");
        }

        var roles = await _rolesService.GetAllAsync(context.TenantId, cancellationToken);
        var role = roles.FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase))
            ?? roles.FirstOrDefault(r => r.Name.Contains(roleName, StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            return Error($"Роль \"{roleName}\" не найдена. Доступные роли: {string.Join(", ", roles.Select(r => r.Name))}.");
        }

        var password = GenerateRandomPassword();

        try
        {
            var user = await _usersService.CreateAsync(
                context.TenantId,
                new CreateUserRequestDto
                {
                    FullName = fullName,
                    Email = email,
                    Password = password,
                    PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                    RoleId = role.Id,
                },
                context.UserId,
                cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                employeeId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = role.Name,
                temporaryPassword = password,
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    private async Task<string> StartInventoryAuditAsync(JsonElement args, AssistantContext context, CancellationToken cancellationToken)
    {
        var warehouseName = GetString(args, "warehouseName");
        var notes = GetString(args, "notes");

        if (string.IsNullOrWhiteSpace(warehouseName))
        {
            return Error("Не указан склад для ревизии.");
        }

        var warehouses = await _warehousesService.GetAllAsync(
            new PagedRequest { PageNumber = 1, PageSize = 100 }, cancellationToken);
        var warehouse = warehouses.Items.FirstOrDefault(w => string.Equals(w.Name, warehouseName, StringComparison.OrdinalIgnoreCase))
            ?? warehouses.Items.FirstOrDefault(w => w.Name.Contains(warehouseName, StringComparison.OrdinalIgnoreCase));

        if (warehouse is null)
        {
            return Error($"Склад \"{warehouseName}\" не найден. Доступные склады: {string.Join(", ", warehouses.Items.Select(w => w.Name))}.");
        }

        try
        {
            var inventory = await _inventoriesService.CreateAsync(
                new CreateInventoryDto { WarehouseId = warehouse.Id, Notes = string.IsNullOrWhiteSpace(notes) ? null : notes },
                cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                inventoryId = inventory.Id,
                inventoryNumber = inventory.InventoryNumber,
                warehouse = warehouse.Name,
                status = inventory.Status.ToString(),
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    private async Task<string> CreateProductAsync(JsonElement args, AssistantContext context, CancellationToken cancellationToken)
    {
        var name = GetString(args, "name");
        var sku = GetString(args, "sku");
        var purchasePrice = GetDecimal(args, "purchasePrice");
        var sellingPrice = GetDecimal(args, "sellingPrice");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku) || purchasePrice is null || sellingPrice is null)
        {
            return Error("Не хватает данных: нужны название, SKU, закупочная и продажная цена.");
        }

        Guid? categoryId = null;
        var categoryName = GetString(args, "categoryName");
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var categories = await _categoriesService.GetAllAsync(context.TenantId, cancellationToken);
            categoryId = (categories.FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase))
                ?? categories.FirstOrDefault(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase)))?.Id;
        }

        Guid? brandId = null;
        var brandName = GetString(args, "brandName");
        if (!string.IsNullOrWhiteSpace(brandName))
        {
            var brands = await _brandsService.GetAllAsync(context.TenantId, cancellationToken);
            brandId = (brands.FirstOrDefault(b => string.Equals(b.Name, brandName, StringComparison.OrdinalIgnoreCase))
                ?? brands.FirstOrDefault(b => b.Name.Contains(brandName, StringComparison.OrdinalIgnoreCase)))?.Id;
        }

        try
        {
            var product = await _productsService.CreateAsync(
                context.TenantId,
                new CreateProductRequestDto
                {
                    Name = name,
                    Sku = sku,
                    Barcode = GetString(args, "barcode"),
                    CategoryId = categoryId,
                    BrandId = brandId,
                    PurchasePrice = purchasePrice.Value,
                    SellingPrice = sellingPrice.Value,
                    Unit = GetString(args, "unit") ?? "шт",
                    MinimumStock = (int)(GetDecimal(args, "minimumStock") ?? 0),
                    TaxRate = GetDecimal(args, "taxRate") ?? 0,
                },
                cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                productId = product.Id,
                name = product.Name,
                sku = product.Sku,
                categoryLinked = categoryId is not null,
                brandLinked = brandId is not null,
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    private async Task<string> CreateRoleAsync(JsonElement args, AssistantContext context, CancellationToken cancellationToken)
    {
        var name = GetString(args, "name");
        var description = GetString(args, "description");
        var requestedNames = GetStringArray(args, "permissionNames");

        if (string.IsNullOrWhiteSpace(name))
        {
            return Error("Не указано название роли.");
        }

        if (requestedNames.Count == 0)
        {
            return Error("Не указаны права для роли.");
        }

        var resolved = await _permissionsRepository.GetByNamesAsync(requestedNames, cancellationToken);
        var unknown = requestedNames.Where(n => !resolved.Any(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase))).ToList();
        if (resolved.Count == 0)
        {
            return Error($"Ни одно из указанных прав не найдено в каталоге: {string.Join(", ", requestedNames)}.");
        }

        try
        {
            var role = await _rolesService.CreateAsync(
                context.TenantId,
                new CreateRoleRequestDto
                {
                    Name = name,
                    Description = description,
                    PermissionIds = resolved.Select(p => p.Id).ToList(),
                },
                cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                roleId = role.Id,
                name = role.Name,
                permissions = role.Permissions,
                ignoredUnknownPermissions = unknown,
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    private async Task<string> CreateStoreAsync(JsonElement args, AssistantContext context, CancellationToken cancellationToken)
    {
        var name = GetString(args, "name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error("Не указано название магазина.");
        }

        try
        {
            var store = await _storesService.CreateAsync(
                context.TenantId,
                new CreateStoreRequestDto
                {
                    Name = name,
                    Address = GetString(args, "address"),
                    Phone = GetString(args, "phone"),
                    Email = GetString(args, "email"),
                    Currency = GetString(args, "currency") ?? "UZS",
                    DefaultTaxRate = GetDecimal(args, "defaultTaxRate") ?? 0,
                },
                cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                storeId = store.Id,
                name = store.Name,
                currency = store.Currency,
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    private static string? GetString(JsonElement args, string property) =>
        args.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static decimal? GetDecimal(JsonElement args, string property)
    {
        if (!args.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var d) => d,
            JsonValueKind.String when decimal.TryParse(value.GetString(), out var d) => d,
            _ => null,
        };
    }

    private static List<string> GetStringArray(JsonElement args, string property)
    {
        if (!args.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return new List<string>();
        }

        return value.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString()!)
            .ToList();
    }

    private static string Error(string message) => JsonSerializer.Serialize(new { error = message });

    private static string GenerateRandomPassword()
    {
        // Не используется как долговременный пароль в проде — сотрудник меняет его после первого входа.
        // 8+ символов достаточно для CreateUserRequestDto-валидатора (MinimumLength(8)).
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(12);
        var sb = new StringBuilder(12);
        foreach (var b in bytes)
        {
            sb.Append(chars[b % chars.Length]);
        }

        return sb.ToString();
    }
}
