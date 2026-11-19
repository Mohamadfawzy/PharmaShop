
using Entities.Models;
using Shared.Models.Dtos.Order;
using Shared.Models.Dtos.Order.order;
using Shared.Responses;
using Shared.Models.Dtos.Delivery;

namespace Contracts;

public interface IOrderRepository
{
    Task AddCustomerPointLotAsync(CustomerPointLot lot, CancellationToken ct);
    Task AddItemsRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct);
    Task AddOrderAsync(Order order, CancellationToken ct);
    Task AddPointTransactionsRangeAsync(IEnumerable<CustomerPointTransaction> transactions, CancellationToken ct);
    Task<int> ClosePrescriptionAsync(int prescriptionId, int customerId, CancellationToken ct);
    Task<int> DecreaseCustomerPointsAsync(int customerId, int points, CancellationToken ct);
    Task<Dictionary<int, ActiveGroupPromotionHit>> GetActiveGroupPromotionsByProductAsync(List<int> productIds, CancellationToken ct);
    Task<Dictionary<int, decimal>> GetActiveProductPromotionPercentAsync(List<int> productIds, CancellationToken ct);
    Task<(bool ok, CustomerAddressSnapshot snap)> GetAddressSnapshotAsync(int addressId, int customerId, CancellationToken ct);
    Task<List<CustomerPointLot>> GetAvailablePointLotsForUpdateAsync(int customerId, DateTime nowUtc, CancellationToken ct);
    Task<HashSet<byte>> GetCartItemSourceTypesAsync(int cartId, int customerId, CancellationToken ct);
    Task<Customer?> GetCustomerForPointsUpdateAsync(int customerId, CancellationToken ct);
    Task<int> GetCustomerPointsAsync(int customerId, CancellationToken ct);
    Task<List<CheckoutLineData>> GetLinesFromCartAsync(int cartId, int customerId, CancellationToken ct);
    Task<List<CheckoutLineData>> GetLinesFromPointsRedemptionCartAsync(int cartId, int customerId, CancellationToken ct);
    Task<List<CheckoutLineData>> GetLinesFromPrescriptionAsync(int prescriptionId, int customerId, CancellationToken ct);
    Task<Order?> GetOrderForStatusUpdateAsync(int orderId, CancellationToken ct);
    Task LockOrderForStatusUpdateAsync(int orderId, CancellationToken ct);
    Task<int?> GetStorePharmacyIdAsync(int storeId, CancellationToken ct);
    Task<Employee?> GetActiveDeliveryAgentAsync(int userId, CancellationToken ct);
    Task<List<DeliveryAgentDto>> SearchDeliveryAgentsAsync(int storeId, int page, int pageSize, CancellationToken ct);
    Task<OrderDeliveryAssignment?> GetActiveDeliveryAssignmentAsync(int orderId, bool tracked, CancellationToken ct);
    Task<DeliveryAgentAssignmentDto?> GetDeliveryAssignmentDetailsAsync(int orderId, CancellationToken ct);
    Task AddDeliveryAssignmentAsync(OrderDeliveryAssignment assignment, CancellationToken ct);
    void UpdateDeliveryAssignment(OrderDeliveryAssignment assignment);
    Task<PagedResult<DeliveryOrderListItemDto>> SearchAssignedDeliveryOrdersAsync(int userId, DeliveryOrdersQueryDto query, CancellationToken ct);
    Task<DeliveryOrderDetailsDto?> GetAssignedDeliveryOrderAsync(int orderId, int userId, CancellationToken ct);
    Task<List<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId, CancellationToken ct);
    Task AddStatusHistoryAsync(OrderStatusHistory history, CancellationToken ct);
    Task<List<CustomerPointLot>> GetPendingPointLotsByOrderAsync(int orderId, CancellationToken ct);
    Task<int> MarkCartCheckedOutAsync(int cartId, int customerId, CancellationToken ct);
    Task<AdminOrderDetailsDto?> GetAdminOrderDetailsAsync(int orderId, CancellationToken ct);
    Task<CustomerOrderDetailsDto?> GetCustomerOrderDetailsAsync(int orderId, int customerId, CancellationToken ct);
    Task<PagedResult<CustomerOrderListItemDto>> SearchCustomerAsync(int customerId, CustomerOrderListQueryDto query, CancellationToken ct);
    Task<PagedResult<AdminOrderListItemDto>> SearchAdminAsync(AdminOrderListQueryDto q, CancellationToken ct);
    void UpdateOrder(Order order);
}
