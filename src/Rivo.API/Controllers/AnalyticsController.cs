using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Analytics.Dtos;
using Rivo.Application.Analytics.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet("sales-trend")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<SalesTrendPointDto>>>> GetSalesTrend(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSalesTrendAsync(from, to, cancellationToken);
        return Ok(ApiResponse<List<SalesTrendPointDto>>.Ok(result));
    }

    [HttpGet("best-sellers")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<ProductRankingDto>>>> GetBestSellers(
        [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetBestSellersAsync(from, to, top, cancellationToken);
        return Ok(ApiResponse<List<ProductRankingDto>>.Ok(result));
    }

    [HttpGet("most-profitable")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<ProductRankingDto>>>> GetMostProfitable(
        [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetMostProfitableAsync(from, to, top, cancellationToken);
        return Ok(ApiResponse<List<ProductRankingDto>>.Ok(result));
    }

    [HttpGet("slow-moving")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<SlowMovingProductDto>>>> GetSlowMoving(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSlowMovingAsync(from, to, cancellationToken);
        return Ok(ApiResponse<List<SlowMovingProductDto>>.Ok(result));
    }

    [HttpGet("dead-stock")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<SlowMovingProductDto>>>> GetDeadStock(
        [FromQuery] DateTime since, CancellationToken cancellationToken)
    {
        var result = await _service.GetDeadStockAsync(since, cancellationToken);
        return Ok(ApiResponse<List<SlowMovingProductDto>>.Ok(result));
    }

    [HttpGet("low-stock")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<LowStockItemDto>>>> GetLowStock(CancellationToken cancellationToken)
    {
        var result = await _service.GetLowStockAsync(cancellationToken);
        return Ok(ApiResponse<List<LowStockItemDto>>.Ok(result));
    }

    [HttpGet("employees")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<EmployeeStatDto>>>> GetEmployeeStats(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetEmployeeStatsAsync(from, to, cancellationToken);
        return Ok(ApiResponse<List<EmployeeStatDto>>.Ok(result));
    }

    [HttpGet("branches")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<List<BranchComparisonDto>>>> GetBranchComparison(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetBranchComparisonAsync(from, to, cancellationToken);
        return Ok(ApiResponse<List<BranchComparisonDto>>.Ok(result));
    }
}
