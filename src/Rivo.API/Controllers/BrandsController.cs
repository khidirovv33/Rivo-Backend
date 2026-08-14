using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Brands.Dtos;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

public class BrandsController : ApiControllerBase
{
    private readonly IBrandsService _brandsService;

    public BrandsController(IBrandsService brandsService)
    {
        _brandsService = brandsService;
    }

    [PermissionAuthorize("Brands.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BrandDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _brandsService.GetAllAsync(TenantId, cancellationToken);
        return Ok(ApiResponse<List<BrandDto>>.Ok(result));
    }

    [PermissionAuthorize("Brands.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _brandsService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<BrandDto>.Ok(result));
    }

    [PermissionAuthorize("Brands.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Create(CreateBrandRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _brandsService.CreateAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<BrandDto>.Ok(result));
    }

    [PermissionAuthorize("Brands.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Update(Guid id, UpdateBrandRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _brandsService.UpdateAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<BrandDto>.Ok(result));
    }

    [PermissionAuthorize("Brands.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _brandsService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Brand deleted."));
    }
}
