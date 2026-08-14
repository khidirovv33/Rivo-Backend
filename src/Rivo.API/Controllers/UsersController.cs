using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Users.Dtos;
using Rivo.Application.Users.Interfaces;

namespace Rivo.API.Controllers;

public class UsersController : ApiControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [PermissionAuthorize("Users.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<UserDto>>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _usersService.GetPagedAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<UserDto>>.Ok(result));
    }

    [PermissionAuthorize("Users.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _usersService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [PermissionAuthorize("Users.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _usersService.CreateAsync(TenantId, request, CurrentUserId, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [PermissionAuthorize("Users.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _usersService.UpdateAsync(TenantId, id, request, CurrentUserId, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [PermissionAuthorize("Users.Update")]
    [HttpPost("{id:guid}/block")]
    public async Task<ActionResult<ApiResponse<object>>> Block(Guid id, CancellationToken cancellationToken)
    {
        await _usersService.BlockAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "User blocked."));
    }

    [PermissionAuthorize("Users.Update")]
    [HttpPost("{id:guid}/unblock")]
    public async Task<ActionResult<ApiResponse<object>>> Unblock(Guid id, CancellationToken cancellationToken)
    {
        await _usersService.UnblockAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "User unblocked."));
    }

    [PermissionAuthorize("Users.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _usersService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "User deleted."));
    }
}
