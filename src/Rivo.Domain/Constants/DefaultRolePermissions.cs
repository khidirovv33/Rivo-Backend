namespace Rivo.Domain.Constants;

/// <summary>Default permission set per system role, applied when a tenant is registered (§4 ТЗ role list).</summary>
public static class DefaultRolePermissions
{
    public static readonly Dictionary<string, string[]> Map = new()
    {
        [RoleNames.Owner] = PermissionNames.All().ToArray(),

        [RoleNames.Admin] = PermissionNames.Users.Concat(PermissionNames.Roles).Concat(PermissionNames.Stores)
            .Concat(PermissionNames.Products).Concat(PermissionNames.Categories).Concat(PermissionNames.Brands)
            .Concat(PermissionNames.Customers).Concat(PermissionNames.Loyalty).Concat(PermissionNames.Sales)
            .Concat(PermissionNames.Payments).Concat(PermissionNames.Inventory).Concat(PermissionNames.Finance).ToArray(),

        [RoleNames.Manager] = PermissionNames.Stores.Concat(PermissionNames.Products).Concat(PermissionNames.Categories)
            .Concat(PermissionNames.Brands).Concat(PermissionNames.Customers).Concat(PermissionNames.Loyalty)
            .Concat(PermissionNames.Sales).Concat(PermissionNames.Payments).Concat(PermissionNames.Inventory).ToArray(),

        [RoleNames.Cashier] = PermissionNames.Sales.Concat(PermissionNames.Payments)
            .Concat(new[] { "Products.Read", "Customers.Read", "Customers.Create", "Loyalty.Read" }).ToArray(),

        [RoleNames.WarehouseWorker] = PermissionNames.Inventory
            .Concat(new[] { "Products.Read", "Products.Update" }).ToArray(),

        [RoleNames.Accountant] = PermissionNames.Finance
            .Concat(new[] { "Sales.Read", "Payments.Read" }).ToArray(),

        [RoleNames.Auditor] = PermissionNames.All().Where(p => p.EndsWith(".Read")).ToArray(),
    };
}
