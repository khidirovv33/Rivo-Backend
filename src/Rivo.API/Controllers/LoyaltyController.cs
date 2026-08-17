using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Loyalty.Dtos;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Domain.Exceptions;

namespace Rivo.API.Controllers;

public class LoyaltyController : ApiControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [PermissionAuthorize("Loyalty.Read")]
    [HttpGet("levels")]
    public async Task<ActionResult<ApiResponse<List<LoyaltyLevelDto>>>> GetLevels(CancellationToken cancellationToken)
    {
        var result = await _loyaltyService.GetLevelsAsync(TenantId, cancellationToken);
        return Ok(ApiResponse<List<LoyaltyLevelDto>>.Ok(result));
    }

    [PermissionAuthorize("Loyalty.Create")]
    [HttpPost("levels")]
    public async Task<ActionResult<ApiResponse<LoyaltyLevelDto>>> CreateLevel(CreateLoyaltyLevelRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _loyaltyService.CreateLevelAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<LoyaltyLevelDto>.Ok(result));
    }

    [PermissionAuthorize("Loyalty.Update")]
    [HttpPut("levels/{id:guid}")]
    public async Task<ActionResult<ApiResponse<LoyaltyLevelDto>>> UpdateLevel(Guid id, UpdateLoyaltyLevelRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _loyaltyService.UpdateLevelAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<LoyaltyLevelDto>.Ok(result));
    }

    [PermissionAuthorize("Loyalty.Delete")]
    [HttpDelete("levels/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLevel(Guid id, CancellationToken cancellationToken)
    {
        await _loyaltyService.DeleteLevelAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Loyalty level deleted."));
    }

    [PermissionAuthorize("Loyalty.Create")]
    [HttpPost("cards")]
    public async Task<ActionResult<ApiResponse<LoyaltyCardDto>>> IssueCard(IssueLoyaltyCardRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _loyaltyService.IssueCardAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<LoyaltyCardDto>.Ok(result));
    }

    [PermissionAuthorize("Loyalty.Read")]
    [HttpGet("cards/by-customer/{customerId:guid}")]
    public async Task<ActionResult<ApiResponse<LoyaltyCardDto>>> GetCardByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _loyaltyService.GetCardByCustomerAsync(TenantId, customerId, cancellationToken)
            ?? throw new NotFoundException("LoyaltyCard", customerId);
        return Ok(ApiResponse<LoyaltyCardDto>.Ok(result));
    }
}
