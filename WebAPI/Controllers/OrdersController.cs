using Contracts.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Models.Dtos.Order;
using Shared.Models.Dtos.Order.order;
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




    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminOrderListQueryDto query, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await orderService.GetAdminOrdersAsync(query, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement:
        // - enforce admin/store authorization
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id,[FromBody] OrderStatusUpdateDto dto,CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await orderService.UpdateOrderStatusAsync(id, dto, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement: get employee id from token and store in status history
    }


}
