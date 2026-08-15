using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Exceptions;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Application.StockMovements.Services;

public class StockMovementsService : IStockMovementsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public StockMovementsService(IApplicationDbContext context, ICurrentUserService currentUser, IAuditService audit)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PaginatedList<StockMovementDto>> GetAllAsync(
        PagedRequest request, Guid? warehouseId, Guid? productId, CancellationToken cancellationToken = default)
    {
        var query = _context.StockMovements.AsNoTracking().AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        var mapped = query.OrderByDescending(x => x.CreatedAt).Select(x => ToDto(x));
        return await PaginatedList<StockMovementDto>.CreateAsync(mapped, request.Page, request.PageSize, cancellationToken);
    }

    public async Task<StockMovementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movement = await _context.StockMovements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(StockMovement), id);
        return ToDto(movement);
    }

    public async Task<StockMovementDto> CreateAsync(CreateStockMovementDto dto, CancellationToken cancellationToken = default)
    {
        var stock = await _context.Stocks.FirstOrDefaultAsync(
            x => x.WarehouseId == dto.WarehouseId && x.ProductId == dto.ProductId && x.ProductVariationId == dto.ProductVariationId,
            cancellationToken);

        if (stock is null)
        {
            stock = new StockEntity
            {
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                ProductVariationId = dto.ProductVariationId,
                SystemQuantity = 0,
                ReservedQuantity = 0,
            };
            _context.Stocks.Add(stock);
        }

        var before = stock.SystemQuantity;
        var after = before + dto.Quantity;

        if (after < 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Quantity"] = [$"Операция приведёт к отрицательному остатку: текущий {before}, дельта {dto.Quantity}."],
            });
        }

        stock.SystemQuantity = after;

        var movement = new StockMovement
        {
            WarehouseId = dto.WarehouseId,
            ProductId = dto.ProductId,
            ProductVariationId = dto.ProductVariationId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            QuantityBefore = before,
            QuantityAfter = after,
            Reason = dto.Reason,
            ReferenceType = dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            CreatedByUserId = _currentUser.UserId,
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            $"StockMovement:{dto.Type}",
            nameof(StockEntity),
            stock.Id.ToString(),
            oldValue: before.ToString(),
            newValue: after.ToString(),
            cancellationToken: cancellationToken);

        return ToDto(movement);
    }

    private static StockMovementDto ToDto(StockMovement movement) => new()
    {
        Id = movement.Id,
        WarehouseId = movement.WarehouseId,
        ProductId = movement.ProductId,
        ProductVariationId = movement.ProductVariationId,
        Type = movement.Type,
        Quantity = movement.Quantity,
        QuantityBefore = movement.QuantityBefore,
        QuantityAfter = movement.QuantityAfter,
        Reason = movement.Reason,
        ReferenceType = movement.ReferenceType,
        ReferenceId = movement.ReferenceId,
        CreatedByUserId = movement.CreatedByUserId,
        CreatedAt = movement.CreatedAt,
    };
}
