namespace Rivo.Domain.Enums;

/// <summary>Типы движения склада (раздел 8 ТЗ): приход, расход, продажа, возврат, списание, корректировка, резервирование, перемещение.</summary>
public enum StockMovementType
{
    Receipt = 1,
    Issue = 2,
    Sale = 3,
    Return = 4,
    WriteOff = 5,
    Adjustment = 6,
    TransferOut = 7,
    TransferIn = 8,
}
