using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Delivery;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin")]
[Authorize(Policy = AppPolicies.CanManageOrders)]
public sealed class AdminDeliveryController : BaseApiController
{
    private readonly IDeliveryService _delivery;
    public AdminDeliveryController(IDeliveryService delivery) => _delivery = delivery;

    [HttpGet("delivery-agents")]
    public async Task<IActionResult> GetAgents([FromQuery] DeliveryAgentQueryDto query, CancellationToken ct)
        => FromAppResponse(await _delivery.GetAgentsAsync(query, ct));

    [HttpGet("orders/{orderId:int}/delivery-agent")]
    public async Task<IActionResult> GetAssignment(int orderId, CancellationToken ct)
        => FromAppResponse(await _delivery.GetAssignmentAsync(orderId, ct));

    [HttpPut("orders/{orderId:int}/delivery-agent")]
    public async Task<IActionResult> Assign(int orderId, [FromBody] AssignDeliveryAgentDto dto, CancellationToken ct)
        => FromAppResponse(await _delivery.AssignAsync(orderId, dto, ct));

    [HttpDelete("orders/{orderId:int}/delivery-agent")]
    public async Task<IActionResult> Unassign(int orderId, CancellationToken ct)
        => FromAppResponse(await _delivery.UnassignAsync(orderId, ct));

    [HttpGet("orders/{orderId:int}/status-history")]
    public async Task<IActionResult> GetStatusHistory(int orderId, CancellationToken ct)
        => FromAppResponse(await _delivery.GetOrderStatusHistoryAsync(orderId, ct));
}
