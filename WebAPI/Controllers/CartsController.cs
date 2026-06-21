using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Enums.Cart;
using Shared.Models.Dtos.Cart;
using WebAPI.Controllers.Admin;

namespace WebAPI.Controllers;

[Route("api/v1/Carts")]
[ApiController]
public class CartsController : AdminBaseApiController
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
    public async Task<IActionResult> GetMyCart([FromQuery] int customerId, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.GetMyCartAsync(customerId, ct);

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

    // Future improvements:
    // - Get CustomerId from token and remove it from dto


    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, [FromQuery] int customerId, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.RemoveItemAsync(cartItemId, customerId, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvements:
        // - Get customerId from token claims instead of query parameter
    }


    [HttpDelete("customers/{customerId:int}")]
    public async Task<IActionResult> ClearCart(int customerId, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await cartService.ClearCartAsync(customerId, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement:
        // - Get CustomerId from token claims instead of body
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







