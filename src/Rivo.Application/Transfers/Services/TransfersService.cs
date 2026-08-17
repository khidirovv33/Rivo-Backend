using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Application.Transfers.Dtos;
using Rivo.Application.Transfers.Interfaces;
using Rivo.Domain.Entities.Transfers;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Transfers.Services;

public class TransfersService : ITransfersService
{
    private readonly IApplicationDbContext _context;
    private readonly IStockMovementsService _stockMovements;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public TransfersService(
        IApplicationDbContext context,
        IStockMovementsService stockMovements,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _context = context;
        _stockMovements = stockMovements;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PaginatedList<TransferDto>> GetAllAsync(
        PagedRequest request, Guid? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _context.Transfers.AsNoTracking().Include(x => x.Items).AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.SourceWarehouseId == warehouseId.Value || x.DestinationWarehouseId == warehouseId.Value);
        }

        query = request.SortDescending ? query.OrderByDescending(x => x.TransferDate) : query.OrderBy(x => x.TransferDate);

        var mapped = query.Select(x => ToDto(x));
        return await PaginatedList<TransferDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<TransferDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await FindAsync(id, cancellationToken);
        return ToDto(transfer);
    }

    public async Task<TransferDto> CreateAsync(CreateTransferDto dto, CancellationToken cancellationToken = default)
    {
        var transferNumber = await GenerateTransferNumberAsync(cancellationToken);

        var transfer = new Transfer
        {
            SourceWarehouseId = dto.SourceWarehouseId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            TransferNumber = transferNumber,
            Status = TransferStatus.Draft,
            TransferDate = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedBy = _currentUser.UserId,
            Items = dto.Items.Select(i => new TransferItem
            {
                ProductId = i.ProductId,
                ProductVariationId = i.ProductVariationId,
                Quantity = i.Quantity,
            }).ToList(),
        };

        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", nameof(Transfer), transfer.Id.ToString(), newValue: transferNumber, cancellationToken: cancellationToken);

        return ToDto(transfer);
    }

    public async Task<TransferDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default) =>
        await TransitionAsync(id, TransferStatus.Draft, TransferStatus.Pending, cancellationToken);

    public async Task<TransferDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default) =>
        await TransitionAsync(id, TransferStatus.Pending, TransferStatus.Approved, cancellationToken);

    public async Task<TransferDto> ShipAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await FindAsync(id, cancellationToken);
        EnsureStatus(transfer, TransferStatus.Approved, TransferStatus.Shipped);

        foreach (var item in transfer.Items)
        {
            await _stockMovements.CreateAsync(new CreateStockMovementDto
            {
                WarehouseId = transfer.SourceWarehouseId,
                ProductId = item.ProductId,
                ProductVariationId = item.ProductVariationId,
                Type = StockMovementType.TransferOut,
                Quantity = -item.Quantity,
                Reason = $"Transfer {transfer.TransferNumber}",
                ReferenceType = "Transfer",
                ReferenceId = transfer.Id,
            }, cancellationToken);
        }

        return await SetStatusAsync(transfer, TransferStatus.Shipped, cancellationToken);
    }

    public async Task<TransferDto> ReceiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await FindAsync(id, cancellationToken);
        EnsureStatus(transfer, TransferStatus.Shipped, TransferStatus.Received);

        foreach (var item in transfer.Items)
        {
            await _stockMovements.CreateAsync(new CreateStockMovementDto
            {
                WarehouseId = transfer.DestinationWarehouseId,
                ProductId = item.ProductId,
                ProductVariationId = item.ProductVariationId,
                Type = StockMovementType.TransferIn,
                Quantity = item.Quantity,
                Reason = $"Transfer {transfer.TransferNumber}",
                ReferenceType = "Transfer",
                ReferenceId = transfer.Id,
            }, cancellationToken);
        }

        return await SetStatusAsync(transfer, TransferStatus.Received, cancellationToken);
    }

    public async Task<TransferDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await FindAsync(id, cancellationToken);

        if (transfer.Status is TransferStatus.Shipped or TransferStatus.Received or TransferStatus.Cancelled)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Нельзя отменить перемещение в статусе {transfer.Status}."],
            });
        }

        return await SetStatusAsync(transfer, TransferStatus.Cancelled, cancellationToken);
    }

    private async Task<TransferDto> TransitionAsync(Guid id, TransferStatus expected, TransferStatus target, CancellationToken cancellationToken)
    {
        var transfer = await FindAsync(id, cancellationToken);
        EnsureStatus(transfer, expected, target);
        return await SetStatusAsync(transfer, target, cancellationToken);
    }

    private async Task<TransferDto> SetStatusAsync(Transfer transfer, TransferStatus newStatus, CancellationToken cancellationToken)
    {
        var oldStatus = transfer.Status;
        transfer.Status = newStatus;
        transfer.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "StatusChange", nameof(Transfer), transfer.Id.ToString(), oldValue: oldStatus.ToString(), newValue: newStatus.ToString(), cancellationToken: cancellationToken);

        return ToDto(transfer);
    }

    private static void EnsureStatus(Transfer transfer, TransferStatus expected, TransferStatus target)
    {
        if (transfer.Status != expected)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Нельзя перевести перемещение из {transfer.Status} в {target} (ожидался {expected})."],
            });
        }
    }

    private async Task<Transfer> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Transfers.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Transfer), id);
    }

    private async Task<string> GenerateTransferNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var countToday = await _context.Transfers.CountAsync(x => x.TransferDate >= today, cancellationToken);
        return $"TR-{today:yyyyMMdd}-{countToday + 1:D4}";
    }

    private static TransferDto ToDto(Transfer transfer) => new()
    {
        Id = transfer.Id,
        SourceWarehouseId = transfer.SourceWarehouseId,
        DestinationWarehouseId = transfer.DestinationWarehouseId,
        TransferNumber = transfer.TransferNumber,
        Status = transfer.Status,
        TransferDate = transfer.TransferDate,
        Notes = transfer.Notes,
        Items = transfer.Items.Select(i => new TransferItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductVariationId = i.ProductVariationId,
            Quantity = i.Quantity,
        }).ToList(),
    };
}
