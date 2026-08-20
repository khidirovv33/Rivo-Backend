using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Accounts.Dtos;
using Rivo.Application.Accounts.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountsService _service;

    public AccountsController(IAccountsService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<AccountDto>>>> GetAll(
        [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<AccountDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AccountDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Finance.Create")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> Create(
        [FromBody] CreateAccountDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AccountDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Finance.Update")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> Update(
        Guid id, [FromBody] UpdateAccountDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<AccountDto>.Ok(result));
    }

    [HttpGet("{id:guid}/transactions")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<AccountTransactionDto>>>> GetTransactions(
        Guid id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetTransactionsAsync(id, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<AccountTransactionDto>>.Ok(result));
    }
}
