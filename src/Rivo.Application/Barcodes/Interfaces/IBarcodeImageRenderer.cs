using Rivo.Domain.Enums;

namespace Rivo.Application.Barcodes.Interfaces;

/// <summary>Рендер штрихкода в PNG для печати этикетки — реализация в Infrastructure/ExternalServices.</summary>
public interface IBarcodeImageRenderer
{
    byte[] RenderPng(string code, BarcodeType type);
}
