using Shared.Models.Dtos.Returns;
using Shared.Responses;

namespace Contracts.IServices;

public interface IReturnRequestService
{
    Task<AppResponse<int>> CancelMyOrderAsync(int orderId, CancelOrderDto dto, CancellationToken ct);
    Task<AppResponse<ReturnRequestDto>> CreateMyReturnAsync(int orderId, CreateReturnRequestDto dto, CancellationToken ct);
    Task<AppResponse<ReturnRequestDto>> GetMyReturnAsync(int orderId, int returnRequestId, CancellationToken ct);
    Task<AppResponse<List<ReturnRequestListItemDto>>> GetAdminReturnsAsync(ReturnRequestQueryDto query, CancellationToken ct);
    Task<AppResponse<ReturnRequestDto>> GetAdminReturnAsync(int returnRequestId, CancellationToken ct);
    Task<AppResponse<ReturnReviewResultDto>> ReviewAsync(int returnRequestId, ReviewReturnRequestDto dto, CancellationToken ct);
}
