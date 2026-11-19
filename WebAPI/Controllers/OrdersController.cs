using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Order;

namespace WebAPI.Controllers;

[Route("api/v1/orders")]
[Authorize(Policy = AppPolicies.Customer)]
public sealed class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto, CancellationToken ct)
        => FromAppResponse(await _orderService.CheckoutAsync(dto, ct));

    [HttpPost("checkout/preview")]
    public async Task<IActionResult> Preview([FromBody] CheckoutPreviewRequestDto dto, CancellationToken ct)
        => FromAppResponse(await _orderService.PreviewAsync(dto, ct));

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] CustomerOrderListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _orderService.GetMyOrdersAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMineById(int id, CancellationToken ct)
        => FromAppResponse(await _orderService.GetMyOrderAsync(id, ct));
}
