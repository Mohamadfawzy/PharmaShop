using Shared.Models.Dtos.Delivery;
using Shared.Responses;

namespace Contracts.IServices;

public interface IDeliveryService
{
    Task<AppResponse<List<DeliveryAgentDto>>> GetAgentsAsync(DeliveryAgentQueryDto query, CancellationToken ct);
    Task<AppResponse<DeliveryAgentAssignmentDto>> AssignAsync(int orderId, AssignDeliveryAgentDto dto, CancellationToken ct);
    Task<AppResponse<int>> UnassignAsync(int orderId, CancellationToken ct);
    Task<AppResponse<DeliveryAgentAssignmentDto>> GetAssignmentAsync(int orderId, CancellationToken ct);
    Task<AppResponse<List<DeliveryOrderListItemDto>>> GetMyOrdersAsync(DeliveryOrdersQueryDto query, CancellationToken ct);
    Task<AppResponse<DeliveryOrderDetailsDto>> GetMyOrderAsync(int orderId, CancellationToken ct);
    Task<AppResponse<int>> UpdateMyOrderStatusAsync(int orderId, UpdateDeliveryOrderStatusDto dto, CancellationToken ct);
    Task<AppResponse<List<OrderStatusHistoryDto>>> GetOrderStatusHistoryAsync(int orderId, CancellationToken ct);
}
