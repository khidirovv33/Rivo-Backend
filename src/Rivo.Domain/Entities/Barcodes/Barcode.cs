using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Barcodes;

/// <summary>Штрихкод товара (раздел 6, 22 ТЗ): поиск, сканирование, генерация, печать этикеток.</summary>
public class Barcode : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>FK -> Product (модуль Dev1).</summary>
    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public string Code { get; set; } = null!;

    public BarcodeType Type { get; set; } = BarcodeType.EAN13;

    /// <summary>Основной штрихкод товара (для этикетки/чека по умолчанию), если их несколько.</summary>
    public bool IsPrimary { get; set; }
}
