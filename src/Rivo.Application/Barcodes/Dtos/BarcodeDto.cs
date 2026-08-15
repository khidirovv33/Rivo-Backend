using Rivo.Domain.Enums;

namespace Rivo.Application.Barcodes.Dtos;

public class BarcodeDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public string Code { get; set; } = null!;

    public BarcodeType Type { get; set; }

    public bool IsPrimary { get; set; }
}

public class GenerateBarcodeDto
{
    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public BarcodeType Type { get; set; } = BarcodeType.EAN13;

    public bool IsPrimary { get; set; }
}

public class RegisterBarcodeDto
{
    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    /// <summary>Уже существующий код (например с упаковки поставщика), а не сгенерированный нами.</summary>
    public string Code { get; set; } = null!;

    public BarcodeType Type { get; set; } = BarcodeType.EAN13;

    public bool IsPrimary { get; set; }
}
