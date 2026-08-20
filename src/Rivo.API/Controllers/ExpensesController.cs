using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Expenses.Dtos;
using Rivo.Application.Expenses.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly IExpensesService _service;

    public ExpensesController(IExpensesService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<ExpenseDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] ExpenseCategory? category, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, from, to, category, cancellationToken);
        return Ok(ApiResponse<PaginatedList<ExpenseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ExpenseDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Finance.Create")]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Create(
        [FromBody] CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ExpenseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Finance.Update")]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Update(
        Guid id, [FromBody] UpdateExpenseDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ExpenseDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Finance.Update")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
