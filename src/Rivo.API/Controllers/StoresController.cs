using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Stores.Dtos;
using Rivo.Application.Stores.Interfaces;

namespace Rivo.API.Controllers;

public class StoresController : ApiControllerBase
{
    private readonly IStoresService _storesService;

    public StoresController(IStoresService storesService)
    {
        _storesService = storesService;
    }

    [PermissionAuthorize("Stores.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StoreDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _storesService.GetAllAsync(TenantId, cancellationToken);
        return Ok(ApiResponse<List<StoreDto>>.Ok(result));
    }

    [PermissionAuthorize("Stores.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StoreDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _storesService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<StoreDto>.Ok(result));
    }

    [PermissionAuthorize("Stores.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StoreDto>>> Create(CreateStoreRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storesService.CreateAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<StoreDto>.Ok(result));
    }

    [PermissionAuthorize("Stores.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StoreDto>>> Update(Guid id, UpdateStoreRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storesService.UpdateAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<StoreDto>.Ok(result));
    }

    [PermissionAuthorize("Stores.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _storesService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Store deleted."));
    }

    [PermissionAuthorize("Stores.Create")]
    [HttpPost("{storeId:guid}/branches")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> AddBranch(Guid storeId, CreateBranchRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storesService.AddBranchAsync(TenantId, storeId, request, cancellationToken);
        return Ok(ApiResponse<BranchDto>.Ok(result));
    }

    [PermissionAuthorize("Stores.Update")]
    [HttpPut("{storeId:guid}/branches/{branchId:guid}")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> UpdateBranch(Guid storeId, Guid branchId, UpdateBranchRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storesService.UpdateBranchAsync(TenantId, storeId, branchId, request, cancellationToken);
        return Ok(ApiResponse<BranchDto>.Ok(result));
    }

    [PermissionAuthorize("Stores.Delete")]
    [HttpDelete("{storeId:guid}/branches/{branchId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBranch(Guid storeId, Guid branchId, CancellationToken cancellationToken)
    {
        await _storesService.DeleteBranchAsync(TenantId, storeId, branchId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Branch deleted."));
    }
}
