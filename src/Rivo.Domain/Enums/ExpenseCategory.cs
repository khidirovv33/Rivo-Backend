namespace Rivo.Domain.Enums;

/// <summary>
/// Раздел 12 ТЗ. "Purchases" здесь — разовые/внеплановые закупки без формального PO (например,
/// офисное оборудование), а не пополнение товарного запаса: формальные PurchaseOrder/Receiving
/// (Dev2) учитываются через COGS в Finance-отчётах, а не дублируются сюда, чтобы не задваивать расход.
/// </summary>
public enum ExpenseCategory
{
    Purchases = 1,
    Rent = 2,
    Salary = 3,
    Transport = 4,
    Utilities = 5,
    Advertising = 6,
    Other = 7,
}
