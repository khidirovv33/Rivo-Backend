using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Suppliers.Dtos;
using Rivo.Application.Suppliers.Interfaces;
using Rivo.Domain.Entities.Suppliers;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Suppliers.Services;

public class SuppliersService : ISuppliersService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public SuppliersService(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<PaginatedList<SupplierDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Name.Contains(request.SearchTerm));
        }

        query = request.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);

        var supplierIds = await query.Select(x => x.Id).ToListAsync(cancellationToken);
        var debts = await GetDebtsAsync(supplierIds, cancellationToken);

        var mapped = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsEnumerable()
            .Select(x => ToDto(x, debts.GetValueOrDefault(x.Id)))
            .ToList();

        return new PaginatedList<SupplierDto>(mapped, supplierIds.Count, request.PageNumber, request.PageSize);
    }

    public async Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await FindAsync(id, cancellationToken);
        var debt = await GetDebtAsync(id, cancellationToken);
        return ToDto(supplier, debt);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            Notes = dto.Notes,
            IsActive = true,
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", nameof(Supplier), supplier.Id.ToString(), newValue: dto.Name, cancellationToken: cancellationToken);

        return ToDto(supplier, 0);
    }

    public async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var supplier = await FindAsync(id, cancellationToken);

        supplier.Name = dto.Name;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.Notes = dto.Notes;
        supplier.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Update", nameof(Supplier), supplier.Id.ToString(), cancellationToken: cancellationToken);

        var debt = await GetDebtAsync(id, cancellationToken);
        return ToDto(supplier, debt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await FindAsync(id, cancellationToken);
        supplier.IsDeleted = true;
        supplier.DeletedAt = DateTime.UtcNow;
        supplier.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Delete", nameof(Supplier), supplier.Id.ToString(), cancellationToken: cancellationToken);
    }

    private async Task<Supplier> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Suppliers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), id);
    }

    private async Task<decimal> GetDebtAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        return await _context.Purchases
            .Where(x => x.SupplierId == supplierId)
            .SumAsync(x => x.TotalAmount - x.PaidAmount, cancellationToken);
    }

    private async Task<Dictionary<Guid, decimal>> GetDebtsAsync(IReadOnlyCollection<Guid> supplierIds, CancellationToken cancellationToken)
    {
        return await _context.Purchases
            .Where(x => supplierIds.Contains(x.SupplierId))
            .GroupBy(x => x.SupplierId)
            .Select(g => new { SupplierId = g.Key, Debt = g.Sum(x => x.TotalAmount - x.PaidAmount) })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Debt, cancellationToken);
    }

    private static SupplierDto ToDto(Supplier supplier, decimal debt) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        ContactPerson = supplier.ContactPerson,
        Phone = supplier.Phone,
        Email = supplier.Email,
        Address = supplier.Address,
        Notes = supplier.Notes,
        IsActive = supplier.IsActive,
        OutstandingDebt = debt,
        CreatedAt = supplier.CreatedAt,
    };
}
