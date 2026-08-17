using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Customers.Dtos;
using Rivo.Application.Customers.Interfaces;

namespace Rivo.API.Controllers;

public class CustomersController : ApiControllerBase
{
    private readonly ICustomersService _customersService;

    public CustomersController(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    [PermissionAuthorize("Customers.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<CustomerDto>>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _customersService.GetPagedAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<CustomerDto>>.Ok(result));
    }

    [PermissionAuthorize("Customers.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customersService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Ok(result));
    }

    [PermissionAuthorize("Customers.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(CreateCustomerRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _customersService.CreateAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Ok(result));
    }

    [PermissionAuthorize("Customers.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(Guid id, UpdateCustomerRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _customersService.UpdateAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Ok(result));
    }

    [PermissionAuthorize("Customers.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customersService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Customer deleted."));
    }
}
