using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Warehouses.Dtos;
using Rivo.Application.Warehouses.Interfaces;
using Rivo.Domain.Exceptions;
using WarehouseEntity = Rivo.Domain.Entities.Warehouses.Warehouse;

namespace Rivo.Application.Warehouses.Services;

public class WarehousesService : IWarehousesService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public WarehousesService(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<PaginatedList<WarehouseDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Warehouses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Name.Contains(request.SearchTerm));
        }

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            _ => request.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
        };

        var mapped = query.Select(x => ToDto(x));
        return await PaginatedList<WarehouseDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<WarehouseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await FindAsync(id, cancellationToken);
        return ToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default)
    {
        var warehouse = new WarehouseEntity
        {
            StoreId = dto.StoreId,
            BranchId = dto.BranchId,
            Name = dto.Name,
            Address = dto.Address,
            IsActive = true,
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", nameof(WarehouseEntity), warehouse.Id.ToString(), newValue: dto.Name, cancellationToken: cancellationToken);

        return ToDto(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default)
    {
        var warehouse = await FindAsync(id, cancellationToken);
        var oldName = warehouse.Name;

        warehouse.Name = dto.Name;
        warehouse.Address = dto.Address;
        warehouse.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Update", nameof(WarehouseEntity), warehouse.Id.ToString(), oldValue: oldName, newValue: dto.Name, cancellationToken: cancellationToken);

        return ToDto(warehouse);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await FindAsync(id, cancellationToken);
        warehouse.IsDeleted = true;
        warehouse.DeletedAt = DateTime.UtcNow;
        warehouse.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Delete", nameof(WarehouseEntity), warehouse.Id.ToString(), cancellationToken: cancellationToken);
    }

    private async Task<WarehouseEntity> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(WarehouseEntity), id);
    }

    private static WarehouseDto ToDto(WarehouseEntity warehouse) => new()
    {
        Id = warehouse.Id,
        StoreId = warehouse.StoreId,
        BranchId = warehouse.BranchId,
        Name = warehouse.Name,
        Address = warehouse.Address,
        IsActive = warehouse.IsActive,
        CreatedAt = warehouse.CreatedAt,
    };
}
