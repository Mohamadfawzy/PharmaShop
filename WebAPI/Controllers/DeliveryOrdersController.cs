using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Delivery;

namespace WebAPI.Controllers;

[Route("api/v1/delivery/orders")]
[Authorize(Policy = AppPolicies.DeliveryAgent)]
[Authorize(Policy = AppPolicies.CanDeliverOrders)]
public sealed class DeliveryOrdersController : BaseApiController
{
    private readonly IDeliveryService _delivery;
    public DeliveryOrdersController(IDeliveryService delivery) => _delivery = delivery;

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] DeliveryOrdersQueryDto query, CancellationToken ct)
        => FromAppResponse(await _delivery.GetMyOrdersAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _delivery.GetMyOrderAsync(id, ct));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDeliveryOrderStatusDto dto, CancellationToken ct)
        => FromAppResponse(await _delivery.UpdateMyOrderStatusAsync(id, dto, ct));
}
