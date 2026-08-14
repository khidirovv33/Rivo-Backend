using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Products.Dtos;
using Rivo.Application.Products.Interfaces;
using Rivo.Domain.Exceptions;

namespace Rivo.API.Controllers;

public class ProductsController : ApiControllerBase
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    [PermissionAuthorize("Products.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductDto>>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _productsService.GetPagedAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<ProductDto>>.Ok(result));
    }

    [PermissionAuthorize("Products.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productsService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    [PermissionAuthorize("Products.Read")]
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> FindByBarcode(string barcode, CancellationToken cancellationToken)
    {
        var result = await _productsService.FindByBarcodeAsync(TenantId, barcode, cancellationToken)
            ?? throw new NotFoundException("Product", barcode);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    [PermissionAuthorize("Products.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productsService.CreateAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    [PermissionAuthorize("Products.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productsService.UpdateAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    [PermissionAuthorize("Products.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productsService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Product deleted."));
    }

    [PermissionAuthorize("Products.Update")]
    [HttpPost("{productId:guid}/variations")]
    public async Task<ActionResult<ApiResponse<ProductVariationDto>>> AddVariation(Guid productId, CreateProductVariationRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productsService.AddVariationAsync(TenantId, productId, request, cancellationToken);
        return Ok(ApiResponse<ProductVariationDto>.Ok(result));
    }

    [PermissionAuthorize("Products.Update")]
    [HttpPut("{productId:guid}/variations/{variationId:guid}")]
    public async Task<ActionResult<ApiResponse<ProductVariationDto>>> UpdateVariation(Guid productId, Guid variationId, UpdateProductVariationRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productsService.UpdateVariationAsync(TenantId, productId, variationId, request, cancellationToken);
        return Ok(ApiResponse<ProductVariationDto>.Ok(result));
    }

    [PermissionAuthorize("Products.Delete")]
    [HttpDelete("{productId:guid}/variations/{variationId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVariation(Guid productId, Guid variationId, CancellationToken cancellationToken)
    {
        await _productsService.DeleteVariationAsync(TenantId, productId, variationId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Variation deleted."));
    }
}
