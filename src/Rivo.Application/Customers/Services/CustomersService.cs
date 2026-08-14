using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Customers.Dtos;
using Rivo.Application.Customers.Interfaces;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Customers.Services;

public class CustomersService : ICustomersService
{
    private readonly ICustomersRepository _customersRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public CustomersService(ICustomersRepository customersRepository, IApplicationDbContext dbContext, IMapper mapper)
    {
        _customersRepository = customersRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<CustomerDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetTenantCustomerOrThrowAsync(tenantId, id, cancellationToken);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<PaginatedList<CustomerDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _customersRepository.GetPagedAsync(
            tenantId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken);

        var dtos = items.Select(c => _mapper.Map<CustomerDto>(c)).ToList();
        return new PaginatedList<CustomerDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<CustomerDto> CreateAsync(Guid tenantId, CreateCustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            BirthDate = request.BirthDate
        };

        await _customersRepository.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid tenantId, Guid id, UpdateCustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        var customer = await GetTenantCustomerOrThrowAsync(tenantId, id, cancellationToken);

        customer.FullName = request.FullName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.BirthDate = request.BirthDate;
        customer.UpdatedAt = DateTime.UtcNow;

        _customersRepository.Update(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetTenantCustomerOrThrowAsync(tenantId, id, cancellationToken);
        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        _customersRepository.Update(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> GetTenantCustomerOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customersRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        if (customer.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return customer;
    }
}
