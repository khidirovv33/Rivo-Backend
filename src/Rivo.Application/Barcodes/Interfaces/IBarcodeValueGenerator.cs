namespace Rivo.Application.Barcodes.Interfaces;

/// <summary>Чистый алгоритм генерации значения штрихкода (без обращения к БД) — реализация в Infrastructure/ExternalServices.</summary>
public interface IBarcodeValueGenerator
{
    string GenerateEan13();
}
