using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Purchases.Dtos;
using Rivo.Application.Purchases.Interfaces;
using Rivo.Domain.Entities.Purchases;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Purchases.Services;

public class PurchasesService : IPurchasesService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public PurchasesService(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<PaginatedList<PurchaseDto>> GetAllAsync(PagedRequest request, Guid? supplierId, CancellationToken cancellationToken = default)
    {
        var query = _context.Purchases.AsNoTracking().AsQueryable();

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        query = request.SortDescending ? query.OrderByDescending(x => x.PurchaseDate) : query.OrderBy(x => x.PurchaseDate);

        var mapped = query.Select(x => ToDto(x));
        return await PaginatedList<PurchaseDto>.CreateAsync(mapped, request.Page, request.PageSize, cancellationToken);
    }

    public async Task<PurchaseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var purchase = await FindAsync(id, cancellationToken);
        return ToDto(purchase);
    }

    public async Task<PurchaseDto> RecordPaymentAsync(Guid id, RecordPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var purchase = await FindAsync(id, cancellationToken);

        if (dto.Amount > purchase.OutstandingAmount)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(dto.Amount)] = [$"Оплата {dto.Amount} превышает задолженность {purchase.OutstandingAmount}."],
            });
        }

        purchase.PaidAmount += dto.Amount;
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "RecordPayment", nameof(Purchase), purchase.Id.ToString(), newValue: dto.Amount.ToString(), cancellationToken: cancellationToken);

        return ToDto(purchase);
    }

    private async Task<Purchase> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Purchases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Purchase), id);
    }

    private static PurchaseDto ToDto(Purchase purchase) => new()
    {
        Id = purchase.Id,
        SupplierId = purchase.SupplierId,
        PurchaseOrderId = purchase.PurchaseOrderId,
        ReceivingId = purchase.ReceivingId,
        PurchaseDate = purchase.PurchaseDate,
        TotalAmount = purchase.TotalAmount,
        PaidAmount = purchase.PaidAmount,
        OutstandingAmount = purchase.OutstandingAmount,
        Notes = purchase.Notes,
    };
}
