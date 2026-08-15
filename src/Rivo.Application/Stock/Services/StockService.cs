using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Stock.Dtos;
using Rivo.Application.Stock.Interfaces;
using Rivo.Domain.Exceptions;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Application.Stock.Services;

public class StockService : IStockService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public StockService(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<PaginatedList<StockDto>> GetAllAsync(PagedRequest request, Guid? warehouseId, Guid? productId, CancellationToken cancellationToken = default)
    {
        var query = _context.Stocks.AsNoTracking().AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        var mapped = query.OrderBy(x => x.CreatedAt).Select(x => ToDto(x));
        return await PaginatedList<StockDto>.CreateAsync(mapped, request.Page, request.PageSize, cancellationToken);
    }

    public async Task<StockDto> GetAsync(Guid warehouseId, Guid productId, Guid? productVariationId, CancellationToken cancellationToken = default)
    {
        var stock = await FindOrDefaultAsync(warehouseId, productId, productVariationId, cancellationToken);
        return ToDto(stock ?? NewStock(warehouseId, productId, productVariationId));
    }

    public async Task<StockDto> ReserveAsync(ReserveStockDto dto, CancellationToken cancellationToken = default)
    {
        var stock = await GetOrCreateAsync(dto.WarehouseId, dto.ProductId, dto.ProductVariationId, cancellationToken);

        if (stock.AvailableQuantity < dto.Quantity)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Quantity"] = [$"Недостаточно доступного остатка: доступно {stock.AvailableQuantity}, запрошено {dto.Quantity}."],
            });
        }

        stock.ReservedQuantity += dto.Quantity;
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Reserve", nameof(StockEntity), stock.Id.ToString(), newValue: dto.Quantity.ToString(), cancellationToken: cancellationToken);

        return ToDto(stock);
    }

    public async Task<StockDto> ReleaseReservationAsync(ReserveStockDto dto, CancellationToken cancellationToken = default)
    {
        var stock = await GetOrCreateAsync(dto.WarehouseId, dto.ProductId, dto.ProductVariationId, cancellationToken);

        stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - dto.Quantity);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ReleaseReservation", nameof(StockEntity), stock.Id.ToString(), newValue: dto.Quantity.ToString(), cancellationToken: cancellationToken);

        return ToDto(stock);
    }

    private async Task<StockEntity> GetOrCreateAsync(Guid warehouseId, Guid productId, Guid? productVariationId, CancellationToken cancellationToken)
    {
        var stock = await FindOrDefaultAsync(warehouseId, productId, productVariationId, cancellationToken);
        if (stock is not null)
        {
            return stock;
        }

        stock = NewStock(warehouseId, productId, productVariationId);
        _context.Stocks.Add(stock);
        return stock;
    }

    private async Task<StockEntity?> FindOrDefaultAsync(Guid warehouseId, Guid productId, Guid? productVariationId, CancellationToken cancellationToken)
    {
        return await _context.Stocks.FirstOrDefaultAsync(
            x => x.WarehouseId == warehouseId && x.ProductId == productId && x.ProductVariationId == productVariationId,
            cancellationToken);
    }

    private static StockEntity NewStock(Guid warehouseId, Guid productId, Guid? productVariationId) => new()
    {
        WarehouseId = warehouseId,
        ProductId = productId,
        ProductVariationId = productVariationId,
        SystemQuantity = 0,
        ReservedQuantity = 0,
    };

    private static StockDto ToDto(StockEntity stock) => new()
    {
        Id = stock.Id,
        WarehouseId = stock.WarehouseId,
        ProductId = stock.ProductId,
        ProductVariationId = stock.ProductVariationId,
        SystemQuantity = stock.SystemQuantity,
        ReservedQuantity = stock.ReservedQuantity,
        AvailableQuantity = stock.AvailableQuantity,
    };
}
