using Entities.Models;
using Shared.Models.Dtos.Points;
using Shared.Responses;

namespace Contracts;

public interface IPointsRepository
{
    Task<PointsBalanceDto?> GetBalanceAsync(int customerId, DateTime nowUtc, CancellationToken ct);
    Task<PagedResult<CustomerPointLotDto>> SearchLotsAsync(int customerId, byte? status, int page, int pageSize, CancellationToken ct);
    Task<PagedResult<CustomerPointTransactionDto>> SearchTransactionsAsync(int customerId, PointsListQueryDto query, CancellationToken ct);
    Task<PagedResult<RedeemablePointsProductDto>> SearchRedeemableProductsAsync(int? storeId, int page, int pageSize, CancellationToken ct);
    Task<bool> AdjustmentReferenceExistsAsync(int referenceId, CancellationToken ct);
    Task<List<CustomerPointLot>> GetSpendableLotsForUpdateAsync(int customerId, DateTime nowUtc, CancellationToken ct);
    Task<List<CustomerPointLot>> GetExpiredLotsForUpdateAsync(DateTime nowUtc, int take, CancellationToken ct);
    Task<List<CustomerPointTransaction>> GetOrderRedemptionsAsync(int orderId, int customerId, CancellationToken ct);
    Task<bool> HasRefundForRedemptionAsync(long sourceTransactionId, CancellationToken ct);
    Task<CustomerPointLot?> GetOrderEarnLotAsync(int orderId, CancellationToken ct);
    Task<int> GetReturnPointAdjustmentTotalAsync(int orderId, byte transactionType, CancellationToken ct);
    Task<int> GetEarnedPointsReversedAsync(int orderItemId, CancellationToken ct);
    Task AddLotAsync(CustomerPointLot lot, CancellationToken ct);
    Task AddTransactionsAsync(IEnumerable<CustomerPointTransaction> transactions, CancellationToken ct);
}
