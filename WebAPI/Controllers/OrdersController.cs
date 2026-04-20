using Contracts.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Models.Dtos.Order;
using WebAPI.Controllers.Admin;

namespace WebAPI.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrdersController : AdminBaseApiController
{
    private readonly IOrderService orderService;

    public OrdersController(IOrderService orderService)
    {
        this.orderService = orderService;
    }



    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await orderService.CheckoutAsync(dto, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }

    [HttpPost("checkout/preview")]
    public async Task<IActionResult> Preview([FromBody] CheckoutPreviewRequestDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await orderService.PreviewAsync(dto, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }



}
