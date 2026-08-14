namespace Rivo.Domain.Constants;

public static class RoleNames
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string WarehouseWorker = "Warehouse Worker";
    public const string Accountant = "Accountant";
    public const string Auditor = "Auditor";

    public static readonly string[] All = { Owner, Admin, Manager, Cashier, WarehouseWorker, Accountant, Auditor };
}
