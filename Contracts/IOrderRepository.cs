
using Entities.Models;
using Shared.Models.Dtos.Order;
using Shared.Models.Dtos.Order.order;
using Shared.Responses;

namespace Contracts;

public interface IOrderRepository
{
    Task AddCustomerPointLotAsync(CustomerPointLot lot, CancellationToken ct);
    Task AddItemsRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct);
    Task AddOrderAsync(Order order, CancellationToken ct);
    Task AddPointTransactionsRangeAsync(IEnumerable<CustomerPointTransaction> transactions, CancellationToken ct);
    Task ClosePrescriptionAsync(int prescriptionId, CancellationToken ct);
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
    Task<List<CustomerPointLot>> GetPendingPointLotsByOrderAsync(int orderId, CancellationToken ct);
    Task MarkCartCheckedOutAsync(int cartId, CancellationToken ct);
    Task<PagedResult<AdminOrderListItemDto>> SearchAdminAsync(AdminOrderListQueryDto q, CancellationToken ct);
    void UpdateOrder(Order order);
}
