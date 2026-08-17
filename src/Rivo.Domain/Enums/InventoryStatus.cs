namespace Rivo.Domain.Enums;

/// <summary>Раздел 11 ТЗ: создать → сканировать → сравнить → подтвердить → скорректировать остатки.</summary>
public enum InventoryStatus
{
    Draft = 1,
    Completed = 2,
    Approved = 3,
    Cancelled = 4,
}
