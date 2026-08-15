using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Barcodes.Dtos;
using Rivo.Application.Barcodes.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Domain.Entities.Barcodes;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Barcodes.Services;

public class BarcodesService : IBarcodesService
{
    private const int MaxGenerationAttempts = 5;

    private readonly IApplicationDbContext _context;
    private readonly IBarcodeValueGenerator _valueGenerator;
    private readonly IBarcodeImageRenderer _imageRenderer;
    private readonly IAuditService _audit;

    public BarcodesService(
        IApplicationDbContext context,
        IBarcodeValueGenerator valueGenerator,
        IBarcodeImageRenderer imageRenderer,
        IAuditService audit)
    {
        _context = context;
        _valueGenerator = valueGenerator;
        _imageRenderer = imageRenderer;
        _audit = audit;
    }

    public async Task<PaginatedList<BarcodeDto>> GetByProductAsync(Guid productId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Barcodes.AsNoTracking().Where(x => x.ProductId == productId);
        var mapped = query.OrderByDescending(x => x.IsPrimary).Select(x => ToDto(x));
        return await PaginatedList<BarcodeDto>.CreateAsync(mapped, request.Page, request.PageSize, cancellationToken);
    }

    public async Task<BarcodeDto> ScanAsync(string code, CancellationToken cancellationToken = default)
    {
        var barcode = await _context.Barcodes.FirstOrDefaultAsync(x => x.Code == code, cancellationToken)
            ?? throw new NotFoundException(nameof(Barcode), code);
        return ToDto(barcode);
    }

    public async Task<BarcodeDto> GenerateAsync(GenerateBarcodeDto dto, CancellationToken cancellationToken = default)
    {
        string code = null!;
        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            var candidate = _valueGenerator.GenerateEan13();
            var exists = await _context.Barcodes.AnyAsync(x => x.Code == candidate, cancellationToken);
            if (!exists)
            {
                code = candidate;
                break;
            }
        }

        if (code is null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Code"] = ["Не удалось сгенерировать уникальный штрихкод, попробуйте снова."],
            });
        }

        return await SaveAsync(dto.ProductId, dto.ProductVariationId, code, dto.Type, dto.IsPrimary, cancellationToken);
    }

    public async Task<BarcodeDto> RegisterAsync(RegisterBarcodeDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Barcodes.AnyAsync(x => x.Code == dto.Code, cancellationToken);
        if (exists)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Code"] = [$"Штрихкод {dto.Code} уже зарегистрирован."],
            });
        }

        return await SaveAsync(dto.ProductId, dto.ProductVariationId, dto.Code, dto.Type, dto.IsPrimary, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var barcode = await FindAsync(id, cancellationToken);
        _context.Barcodes.Remove(barcode);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Delete", nameof(Barcode), barcode.Id.ToString(), oldValue: barcode.Code, cancellationToken: cancellationToken);
    }

    public async Task<byte[]> GetLabelImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var barcode = await FindAsync(id, cancellationToken);
        return _imageRenderer.RenderPng(barcode.Code, barcode.Type);
    }

    private async Task<BarcodeDto> SaveAsync(
        Guid productId, Guid? productVariationId, string code, Domain.Enums.BarcodeType type, bool isPrimary, CancellationToken cancellationToken)
    {
        if (isPrimary)
        {
            var existingPrimaries = await _context.Barcodes
                .Where(x => x.ProductId == productId && x.ProductVariationId == productVariationId && x.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var existing in existingPrimaries)
            {
                existing.IsPrimary = false;
            }
        }

        var barcode = new Barcode
        {
            ProductId = productId,
            ProductVariationId = productVariationId,
            Code = code,
            Type = type,
            IsPrimary = isPrimary,
        };

        _context.Barcodes.Add(barcode);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", nameof(Barcode), barcode.Id.ToString(), newValue: code, cancellationToken: cancellationToken);

        return ToDto(barcode);
    }

    private async Task<Barcode> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Barcodes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Barcode), id);
    }

    private static BarcodeDto ToDto(Barcode barcode) => new()
    {
        Id = barcode.Id,
        ProductId = barcode.ProductId,
        ProductVariationId = barcode.ProductVariationId,
        Code = barcode.Code,
        Type = barcode.Type,
        IsPrimary = barcode.IsPrimary,
    };
}
