using Rivo.Application.Barcodes.Interfaces;
using Rivo.Domain.Enums;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>Генерация значения EAN-13 и рендер PNG для печати этикеток (раздел 6, 22 ТЗ).</summary>
public class BarcodeGeneratorService : IBarcodeValueGenerator, IBarcodeImageRenderer
{
    public string GenerateEan13()
    {
        var digits = new int[12];
        for (var i = 0; i < digits.Length; i++)
        {
            digits[i] = Random.Shared.Next(0, 10);
        }

        var checkDigit = CalculateEan13CheckDigit(digits);
        return string.Concat(digits) + checkDigit;
    }

    public byte[] RenderPng(string code, BarcodeType type)
    {
        var writer = new BarcodeWriter
        {
            Format = MapFormat(type),
            Options = new EncodingOptions
            {
                Width = 300,
                Height = 150,
                Margin = 10,
            },
        };

        using var bitmap = writer.Write(code);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static BarcodeFormat MapFormat(BarcodeType type) => type switch
    {
        BarcodeType.EAN13 => BarcodeFormat.EAN_13,
        BarcodeType.Code128 => BarcodeFormat.CODE_128,
        BarcodeType.QRCode => BarcodeFormat.QR_CODE,
        _ => BarcodeFormat.EAN_13,
    };

    private static int CalculateEan13CheckDigit(int[] first12Digits)
    {
        var sum = 0;
        for (var i = 0; i < first12Digits.Length; i++)
        {
            sum += first12Digits[i] * (i % 2 == 0 ? 1 : 3);
        }

        var remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }
}
