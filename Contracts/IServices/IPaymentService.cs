using Shared.Models.Dtos.Payments;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPaymentService
{
    Task<AppResponse<List<PaymentAttemptDto>>> GetOrderPaymentsAsync(int orderId, CancellationToken ct);
    Task<AppResponse<PaymentAttemptDto>> CreateAttemptAsync(int orderId, CreatePaymentAttemptDto dto, CancellationToken ct);
    Task<AppResponse<PaymentAttemptDto>> UpdateAttemptAsync(int attemptId, UpdatePaymentAttemptDto dto, CancellationToken ct);
    Task<AppResponse<PaymentRefundDto>> CreateRefundAsync(int orderId, CreatePaymentRefundDto dto, CancellationToken ct);
    Task<AppResponse<PaymentRefundDto>> UpdateRefundAsync(int refundId, UpdatePaymentRefundDto dto, CancellationToken ct);
}
