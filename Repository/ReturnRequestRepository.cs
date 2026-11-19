using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Returns;

namespace Repository;

public sealed class ReturnRequestRepository : IReturnRequestRepository
{
    private readonly RepositoryContext _db;
    public ReturnRequestRepository(RepositoryContext db) => _db = db;

    public async Task LockOrderForReturnsAsync(int orderId, CancellationToken ct)
        => _ = await _db.Orders.FromSqlInterpolated($"SELECT * FROM [dbo].[Orders] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {orderId}")
            .Select(x => x.Id).SingleOrDefaultAsync(ct);

    public async Task LockRequestForReviewAsync(int returnRequestId, CancellationToken ct)
        => _ = await _db.ReturnRequests.FromSqlInterpolated($"SELECT * FROM [dbo].[ReturnRequests] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {returnRequestId}")
            .Select(x => x.Id).SingleOrDefaultAsync(ct);

    public Task<Order?> GetCustomerOrderAsync(int orderId, int customerId, bool tracked, CancellationToken ct)
    {
        var query = _db.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Where(o => o.Id == orderId && o.CustomerId == customerId);
        if (!tracked) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public Task<ReturnRequest?> GetByIdAsync(int id, bool tracked, CancellationToken ct)
    {
        var query = _db.ReturnRequests.Include(x => x.Items).ThenInclude(i => i.OrderItem).ThenInclude(i => i.Product)
            .Include(x => x.Order).ThenInclude(o => o.Customer)
            .Include(x => x.Order).ThenInclude(o => o.OrderItems)
            .Include(x => x.Order).ThenInclude(o => o.Store)
            .Include(x => x.ReviewedByEmployee)
            .Where(x => x.Id == id);
        if (!tracked) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<int, decimal>> GetReservedQuantitiesAsync(IReadOnlyCollection<int> orderItemIds, CancellationToken ct)
        => await _db.ReturnRequestItems.AsNoTracking()
            .Where(i => orderItemIds.Contains(i.OrderItemId) && (i.ReturnRequest.Status == 1 || i.ReturnRequest.Status == 2))
            .GroupBy(i => i.OrderItemId).Select(g => new { OrderItemId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.OrderItemId, x => x.Quantity, ct);

    public Task<List<ReturnRequestItem>> GetApprovedItemsAsync(int orderId, int excludingReturnId, CancellationToken ct)
        => _db.ReturnRequestItems.AsNoTracking().Include(i => i.OrderItem)
            .Where(i => i.ReturnRequest.OrderId == orderId && i.ReturnRequest.Status == 2 && i.ReturnRequestId != excludingReturnId)
            .ToListAsync(ct);

    public async Task<(List<ReturnRequestListItemDto> Items, int TotalCount)> SearchAsync(ReturnRequestQueryDto query, int? pharmacyId, CancellationToken ct)
    {
        var source = _db.ReturnRequests.AsNoTracking().AsQueryable();
        if (pharmacyId.HasValue) source = source.Where(x => x.Order.Store.PharmacyId == pharmacyId.Value);
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status.Value);
        if (query.OrderId.HasValue) source = source.Where(x => x.OrderId == query.OrderId.Value);
        if (query.CustomerId.HasValue) source = source.Where(x => x.CustomerId == query.CustomerId.Value);
        if (query.FromUtc.HasValue) source = source.Where(x => x.RequestedAt >= query.FromUtc.Value);
        if (query.ToUtc.HasValue) source = source.Where(x => x.RequestedAt < query.ToUtc.Value);
        var total = await source.CountAsync(ct);
        var items = await source.OrderByDescending(x => x.RequestedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new ReturnRequestListItemDto
            {
                Id = x.Id, OrderId = x.OrderId, OrderNumber = x.Order.OrderNumber, CustomerId = x.CustomerId,
                CustomerNameAr = x.Customer.FullNameAr, Status = x.Status, Reason = x.Reason,
                RequestedAt = x.RequestedAt, ItemsCount = x.Items.Count
            }).ToListAsync(ct);
        return (items, total);
    }

    public Task AddAsync(ReturnRequest request, CancellationToken ct) => _db.ReturnRequests.AddAsync(request, ct).AsTask();
}
