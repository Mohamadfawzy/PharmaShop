using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Returns;

namespace WebAPI.Controllers;

[Route("api/v1/orders/{orderId:int}")]
[Authorize(Policy = AppPolicies.Customer)]
public sealed class CustomerOrderReturnsController : BaseApiController
{
    private readonly IReturnRequestService _returns;
    public CustomerOrderReturnsController(IReturnRequestService returns) => _returns = returns;

    [HttpPatch("cancel")]
    public async Task<IActionResult> Cancel(int orderId, [FromBody] CancelOrderDto dto, CancellationToken ct)
        => FromAppResponse(await _returns.CancelMyOrderAsync(orderId, dto, ct));

    [HttpPost("returns")]
    public async Task<IActionResult> CreateReturn(int orderId, [FromBody] CreateReturnRequestDto dto, CancellationToken ct)
        => FromAppResponse(await _returns.CreateMyReturnAsync(orderId, dto, ct));

    [HttpGet("returns/{returnRequestId:int}")]
    public async Task<IActionResult> GetReturn(int orderId, int returnRequestId, CancellationToken ct)
        => FromAppResponse(await _returns.GetMyReturnAsync(orderId, returnRequestId, ct));
}
