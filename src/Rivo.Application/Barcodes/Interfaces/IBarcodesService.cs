using Rivo.Application.Barcodes.Dtos;
using Rivo.Application.Common.Models;

namespace Rivo.Application.Barcodes.Interfaces;

public interface IBarcodesService
{
    Task<PaginatedList<BarcodeDto>> GetByProductAsync(Guid productId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>Поиск товара по отсканированному коду.</summary>
    Task<BarcodeDto> ScanAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Генерирует новый уникальный код (EAN-13) и привязывает его к товару.</summary>
    Task<BarcodeDto> GenerateAsync(GenerateBarcodeDto dto, CancellationToken cancellationToken = default);

    /// <summary>Регистрирует уже существующий код (полученный, например, от поставщика).</summary>
    Task<BarcodeDto> RegisterAsync(RegisterBarcodeDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>PNG-изображение штрихкода для печати этикетки.</summary>
    Task<byte[]> GetLabelImageAsync(Guid id, CancellationToken cancellationToken = default);
}
