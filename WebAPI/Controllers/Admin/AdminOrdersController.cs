using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Order.order;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/orders")]
[Authorize(Policy = AppPolicies.CanManageOrders)]
public sealed class AdminOrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    public AdminOrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminOrderListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _orderService.GetAdminOrdersAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _orderService.GetAdminOrderAsync(id, ct));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto, CancellationToken ct)
        => FromAppResponse(await _orderService.UpdateOrderStatusAsync(id, dto, ct));
}
