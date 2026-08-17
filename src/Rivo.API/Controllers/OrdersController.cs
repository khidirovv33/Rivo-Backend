using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Orders.Dtos;
using Rivo.Application.Orders.Interfaces;

namespace Rivo.API.Controllers;

public class OrdersController : ApiControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [PermissionAuthorize("Sales.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<OrderDto>>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _ordersService.GetPagedAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<OrderDto>>.Ok(result));
    }

    [PermissionAuthorize("Sales.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ordersService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }
}
