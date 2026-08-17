using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Categories.Dtos;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

public class CategoriesController : ApiControllerBase
{
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    [PermissionAuthorize("Categories.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _categoriesService.GetAllAsync(TenantId, cancellationToken);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    [PermissionAuthorize("Categories.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoriesService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [PermissionAuthorize("Categories.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _categoriesService.CreateAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [PermissionAuthorize("Categories.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _categoriesService.UpdateAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [PermissionAuthorize("Categories.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _categoriesService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Category deleted."));
    }
}
