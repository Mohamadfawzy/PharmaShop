
using Entities.Models;
using Shared.Models.Dtos.Order;

namespace Contracts;

public interface IOrderRepository
{
    Task AddItemsRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct);
    Task AddOrderAsync(Order order, CancellationToken ct);
    Task ClosePrescriptionAsync(int prescriptionId, CancellationToken ct);
    Task<Dictionary<int, ActiveGroupPromotionHit>> GetActiveGroupPromotionsByProductAsync(List<int> productIds, CancellationToken ct);
    Task<Dictionary<int, decimal>> GetActiveProductPromotionPercentAsync(List<int> productIds, CancellationToken ct);
    Task<(bool ok, CustomerAddressSnapshot snap)> GetAddressSnapshotAsync(int addressId, int customerId, CancellationToken ct);
    Task<List<CheckoutLineData>> GetLinesFromCartAsync(int cartId, int customerId, CancellationToken ct);
    Task<List<CheckoutLineData>> GetLinesFromPrescriptionAsync(int prescriptionId, int customerId, CancellationToken ct);
    Task MarkCartCheckedOutAsync(int cartId, CancellationToken ct);
}
