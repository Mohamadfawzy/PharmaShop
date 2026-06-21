using Shared.Models.Dtos.Order;
using Shared.Models.Dtos.Order.order;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;

public interface IOrderService
{
    Task<AppResponse<CheckoutResultDto>> CheckoutAsync(CheckoutRequestDto dto, CancellationToken ct);
    Task<AppResponse<List<AdminOrderListItemDto>>> GetAdminOrdersAsync(AdminOrderListQueryDto query, CancellationToken ct);
    Task<AppResponse<CheckoutPreviewResponseDto>> PreviewAsync(CheckoutPreviewRequestDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDto dto, CancellationToken ct);
}
