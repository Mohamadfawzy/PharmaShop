using Entities.Models;
using Shared.Models.Dtos.Inventory;

namespace Contracts;

public interface IInventoryRepository
{
    Task<Product?> GetProductAsync(int productId, CancellationToken ct);
    Task<List<OrderItem>> GetOrderItemsAsync(int orderId, CancellationToken ct);
    Task<bool> HasOrderMovementAsync(int orderId, byte transactionType, CancellationToken ct);
    Task<bool> HasReturnMovementAsync(int returnRequestItemId, CancellationToken ct);
    Task<List<ProductInventoryTransaction>> GetOrderMovementsAsync(int orderId, byte transactionType, CancellationToken ct);
    Task<bool> ApplyStockDeltaAsync(int productId, decimal expectedQuantity, decimal delta, CancellationToken ct);
    Task AddMovementAsync(ProductInventoryTransaction movement, CancellationToken ct);
    Task<(List<InventoryTransactionDto> Items, int TotalCount)> SearchAsync(InventoryTransactionQueryDto query, int? pharmacyId, CancellationToken ct);
}
