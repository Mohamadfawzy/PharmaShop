using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums.Cart;
using Shared.Models.Dtos.Cart;
using Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers;

[Route("api/v1/cart")]
[ApiController]
[Authorize(Policy = AppPolicies.Customer)]
public class CartsController : BaseApiController
{
    private readonly ICartService cartService;

    public CartsController(ICartService cartService)
    {
        this.cartService = cartService;
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddNormalItem(
            [FromBody] CartAddItemDto dto,
            CancellationToken ct)
    {
        // 1) Add normal item
        var result = await cartService.AddItemAsync(dto, CartItemSourceType.Normal, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }

    [HttpPost("redeem/items")]
    public async Task<IActionResult> AddRedeemItem(
        [FromBody] CartAddItemDto dto,
        CancellationToken ct)
    {
        // 1) Add points redemption item
        var result = await cartService.AddItemAsync(dto, CartItemSourceType.PointsRedemption, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }



    [HttpGet]
    public async Task<IActionResult> GetMyCart(CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.GetMyCartAsync(ct);

        // 2) Unified response
        return FromAppResponse(result);
    }



    [HttpPatch("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItemQty(int cartItemId, [FromBody] CartUpdateQtyDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.UpdateItemQtyAsync(cartItemId, dto, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.RemoveItemAsync(cartItemId, ct);

        // 2) Unified response
        return FromAppResponse(result);

    }


    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.ClearCartAsync(ct);

        // 2) Unified response
        return FromAppResponse(result);

    }



    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] CartPreviewRequestDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.PreviewAsync(dto, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }

}







