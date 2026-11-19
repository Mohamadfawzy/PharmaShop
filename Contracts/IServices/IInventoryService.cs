using Shared.Models.Dtos.Inventory;
using Shared.Responses;

namespace Contracts.IServices;

public interface IInventoryService
{
    Task<AppResponse<InventoryTransactionDto>> ReceiveAsync(int productId, Shared.Models.Dtos.Product.ReceiveStockDto dto, CancellationToken ct);
    Task<AppResponse<InventoryTransactionDto>> AdjustAsync(AdjustInventoryDto dto, CancellationToken ct);
    Task<AppResponse<InventoryTransactionPageDto>> SearchAsync(InventoryTransactionQueryDto query, CancellationToken ct);
    Task ApplyOrderConfirmationAsync(int orderId, CancellationToken ct);
    Task RestoreOrderStockAsync(int orderId, CancellationToken ct);
}
