namespace Rivo.Domain.Constants;

/// <summary>Global permission catalog seeded once at startup (Domain/Common/§4 ТЗ). Inventory.* and Finance.* are listed here per the shared catalog but enforced by Dev2/Dev3 modules, not Dev1.</summary>
public static class PermissionNames
{
    public static readonly string[] Users = { "Users.Read", "Users.Create", "Users.Update", "Users.Delete" };
    public static readonly string[] Roles = { "Roles.Read", "Roles.Create", "Roles.Update", "Roles.Delete" };
    public static readonly string[] Stores = { "Stores.Read", "Stores.Create", "Stores.Update", "Stores.Delete" };
    public static readonly string[] Products = { "Products.Read", "Products.Create", "Products.Update", "Products.Delete" };
    public static readonly string[] Categories = { "Categories.Read", "Categories.Create", "Categories.Update", "Categories.Delete" };
    public static readonly string[] Brands = { "Brands.Read", "Brands.Create", "Brands.Update", "Brands.Delete" };
    public static readonly string[] Customers = { "Customers.Read", "Customers.Create", "Customers.Update", "Customers.Delete" };
    public static readonly string[] Loyalty = { "Loyalty.Read", "Loyalty.Create", "Loyalty.Update", "Loyalty.Delete" };
    public static readonly string[] Sales = { "Sales.Read", "Sales.Create", "Sales.Return" };
    public static readonly string[] Payments = { "Payments.Read", "Payments.Create" };
    public static readonly string[] Inventory = { "Inventory.Read", "Inventory.Create", "Inventory.Approve" };
    public static readonly string[] Finance = { "Finance.Read", "Finance.Create", "Finance.Update" };

    public static IEnumerable<string> All() =>
        Users.Concat(Roles).Concat(Stores).Concat(Products).Concat(Categories).Concat(Brands)
            .Concat(Customers).Concat(Loyalty).Concat(Sales).Concat(Payments).Concat(Inventory).Concat(Finance);
}
