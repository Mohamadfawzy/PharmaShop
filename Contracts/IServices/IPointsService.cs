using Shared.Models.Dtos.Points;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPointsService
{
    Task<AppResponse<PointsBalanceDto>> GetMyBalanceAsync(CancellationToken ct);
    Task<AppResponse<List<CustomerPointLotDto>>> GetMyLotsAsync(PointsListQueryDto query, CancellationToken ct);
    Task<AppResponse<List<CustomerPointTransactionDto>>> GetMyTransactionsAsync(PointsListQueryDto query, CancellationToken ct);
    Task<AppResponse<List<RedeemablePointsProductDto>>> GetRedeemableProductsAsync(int? storeId, int page, int pageSize, CancellationToken ct);
    Task<AppResponse<ManualPointsAdjustmentResultDto>> AdjustCustomerPointsAsync(int customerId, ManualPointsAdjustmentDto dto, CancellationToken ct);
    Task<int> ExpireDuePointsAsync(CancellationToken ct);
    Task RestoreRedeemedPointsAsync(Entities.Models.Order order, DateTime refundedAtUtc, CancellationToken ct);
}
