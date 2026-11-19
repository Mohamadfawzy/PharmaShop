using Entities.Models;
using Shared.Models.Dtos.Returns;

namespace Contracts;

public interface IReturnRequestRepository
{
    Task LockOrderForReturnsAsync(int orderId, CancellationToken ct);
    Task LockRequestForReviewAsync(int returnRequestId, CancellationToken ct);
    Task<Order?> GetCustomerOrderAsync(int orderId, int customerId, bool tracked, CancellationToken ct);
    Task<ReturnRequest?> GetByIdAsync(int id, bool tracked, CancellationToken ct);
    Task<Dictionary<int, decimal>> GetReservedQuantitiesAsync(IReadOnlyCollection<int> orderItemIds, CancellationToken ct);
    Task<List<ReturnRequestItem>> GetApprovedItemsAsync(int orderId, int excludingReturnId, CancellationToken ct);
    Task<(List<ReturnRequestListItemDto> Items, int TotalCount)> SearchAsync(ReturnRequestQueryDto query, int? pharmacyId, CancellationToken ct);
    Task AddAsync(ReturnRequest request, CancellationToken ct);
}
