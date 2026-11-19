using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Payments;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin")]
[Authorize(Policy = AppPolicies.CanManageOrders)]
public sealed class AdminPaymentsController : BaseApiController
{
    private readonly IPaymentService _payments;
    public AdminPaymentsController(IPaymentService payments) => _payments = payments;

    [HttpGet("orders/{orderId:int}/payments")]
    public async Task<IActionResult> GetOrderPayments(int orderId, CancellationToken ct)
        => FromAppResponse(await _payments.GetOrderPaymentsAsync(orderId, ct));
    [HttpPost("orders/{orderId:int}/payment-attempts")]
    public async Task<IActionResult> CreateAttempt(int orderId, [FromBody] CreatePaymentAttemptDto dto, CancellationToken ct)
        => FromAppResponse(await _payments.CreateAttemptAsync(orderId, dto, ct));
    [HttpPatch("payment-attempts/{attemptId:int}")]
    public async Task<IActionResult> UpdateAttempt(int attemptId, [FromBody] UpdatePaymentAttemptDto dto, CancellationToken ct)
        => FromAppResponse(await _payments.UpdateAttemptAsync(attemptId, dto, ct));
    [HttpPost("orders/{orderId:int}/refunds")]
    public async Task<IActionResult> CreateRefund(int orderId, [FromBody] CreatePaymentRefundDto dto, CancellationToken ct)
        => FromAppResponse(await _payments.CreateRefundAsync(orderId, dto, ct));
    [HttpPatch("payment-refunds/{refundId:int}")]
    public async Task<IActionResult> UpdateRefund(int refundId, [FromBody] UpdatePaymentRefundDto dto, CancellationToken ct)
        => FromAppResponse(await _payments.UpdateRefundAsync(refundId, dto, ct));
}
