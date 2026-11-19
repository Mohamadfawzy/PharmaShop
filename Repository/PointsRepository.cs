using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Points;
using Shared.Responses;

namespace Repository;

public sealed class PointsRepository : IPointsRepository
{
    private readonly RepositoryContext _db;
    public PointsRepository(RepositoryContext db) => _db = db;

    public async Task<PointsBalanceDto?> GetBalanceAsync(int customerId, DateTime nowUtc, CancellationToken ct)
    {
        var customer = await _db.Customers.AsNoTracking().Where(c => c.Id == customerId && c.IsActive)
            .Select(c => new { c.Points }).FirstOrDefaultAsync(ct);
        if (customer is null) return null;
        var lots = _db.CustomerPointLots.AsNoTracking().Where(l => l.CustomerId == customerId && l.RemainingPoints > 0);
        var pending = await lots.Where(l => l.Status == 1).SumAsync(l => (int?)l.RemainingPoints, ct) ?? 0;
        var active = lots.Where(l => l.Status == 2 && l.ExpiresAt > nowUtc);
        var activeBalance = await active.SumAsync(l => (int?)l.RemainingPoints, ct) ?? 0;
        var expiring = await active.Where(l => l.ExpiresAt <= nowUtc.AddDays(30)).SumAsync(l => (int?)l.RemainingPoints, ct) ?? 0;
        var nextExpiry = await active.OrderBy(l => l.ExpiresAt).Select(l => (DateTime?)l.ExpiresAt).FirstOrDefaultAsync(ct);
        return new PointsBalanceDto
        {
            AvailablePoints = activeBalance,
            PendingPoints = pending,
            ExpiringWithin30Days = expiring,
            NextExpiryAt = nextExpiry,
            AvailableValueEGP = Math.Round(activeBalance / 30m, 2, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<PagedResult<CustomerPointLotDto>> SearchLotsAsync(int customerId, byte? status, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.CustomerPointLots.AsNoTracking().Where(l => l.CustomerId == customerId);
        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(l => l.ExpiresAt).ThenByDescending(l => l.EarnedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new CustomerPointLotDto { Id = l.Id, OrderId = l.OrderId, PointsTotal = l.PointsTotal,
                RemainingPoints = l.RemainingPoints, Status = l.Status, EarnedAt = l.EarnedAt,
                AvailableAt = l.AvailableAt, ExpiresAt = l.ExpiresAt }).ToListAsync(ct);
        return new PagedResult<CustomerPointLotDto> { Items = items, TotalCount = total };
    }

    public async Task<PagedResult<CustomerPointTransactionDto>> SearchTransactionsAsync(int customerId, PointsListQueryDto q, CancellationToken ct)
    {
        var query = _db.CustomerPointTransactions.AsNoTracking().Where(t => t.CustomerId == customerId);
        if (q.TransactionType.HasValue) query = query.Where(t => t.TransactionType == q.TransactionType.Value);
        if (q.From.HasValue) query = query.Where(t => t.CreatedAt >= q.From.Value);
        if (q.To.HasValue) query = query.Where(t => t.CreatedAt <= q.To.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(t => new CustomerPointTransactionDto { Id = t.Id, TransactionType = t.TransactionType,
                PointsDelta = t.PointsDelta, BalanceAfter = t.BalanceAfter, OrderId = t.OrderId,
                OrderItemId = t.OrderItemId, LotId = t.LotId, ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId, ExpiresAt = t.ExpiresAt, Notes = t.Notes, CreatedAt = t.CreatedAt })
            .ToListAsync(ct);
        return new PagedResult<CustomerPointTransactionDto> { Items = items, TotalCount = total };
    }

    public async Task<PagedResult<RedeemablePointsProductDto>> SearchRedeemableProductsAsync(int? storeId, int page, int pageSize, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var query = _db.Products.AsNoTracking().Where(p => p.IsRedeemableByPoints && p.IsActive && p.IsAvailable &&
            p.DeletedAt == null && p.Quantity > 0 && (!storeId.HasValue || p.StoreId == storeId.Value) &&
            !(p.HasPromotion && p.PromotionStartsAt <= now && p.PromotionEndsAt >= now));

        // Keep eligible products out of active group promotions to match the existing cart rule.
        query = query.Where(p => !p.PromotionProducts.Any(pp => pp.DeletedAt == null && pp.Promotion.DeletedAt == null &&
            pp.Promotion.IsActive && pp.Promotion.StartAt <= now && pp.Promotion.EndAt >= now &&
            pp.Promotion.BasicAmount != null && pp.Promotion.OfferAmount != null && pp.Promotion.DiscountPercent != null));

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(p => p.NameAr).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new RedeemablePointsProductDto { Id = p.Id, StoreId = p.StoreId, NameAr = p.NameAr,
                NameEn = p.NameEn, OuterUnitPrice = p.OuterUnitPrice, InnerUnitPrice = p.InnerUnitPrice,
                AvailableQuantity = p.Quantity, OuterUnitId = p.OuterUnitId, InnerUnitId = p.InnerUnitId,
                InnerPerOuter = p.InnerPerOuter, AllowSplitSale = p.AllowSplitSale }).ToListAsync(ct);
        return new PagedResult<RedeemablePointsProductDto> { Items = items, TotalCount = total };
    }

    public Task<bool> AdjustmentReferenceExistsAsync(int referenceId, CancellationToken ct)
        => _db.CustomerPointTransactions.AsNoTracking().AnyAsync(t => t.TransactionType == 7 &&
            t.ReferenceType == 3 && t.ReferenceId == referenceId, ct);

    public Task<List<CustomerPointLot>> GetSpendableLotsForUpdateAsync(int customerId, DateTime nowUtc, CancellationToken ct)
        => _db.CustomerPointLots.FromSqlInterpolated($"SELECT * FROM [dbo].[CustomerPointLots] WITH (UPDLOCK, ROWLOCK, READPAST) WHERE [CustomerId] = {customerId} AND [Status] = 2 AND [RemainingPoints] > 0 AND [ExpiresAt] > {nowUtc}")
            .OrderBy(l => l.ExpiresAt).ThenBy(l => l.Id).ToListAsync(ct);

    public Task<List<CustomerPointLot>> GetExpiredLotsForUpdateAsync(DateTime nowUtc, int take, CancellationToken ct)
        => _db.CustomerPointLots.FromSqlInterpolated($"SELECT TOP ({take}) * FROM [dbo].[CustomerPointLots] WITH (UPDLOCK, ROWLOCK, READPAST) WHERE [Status] = 2 AND [RemainingPoints] > 0 AND [ExpiresAt] <= {nowUtc} ORDER BY [ExpiresAt], [Id]")
            .ToListAsync(ct);

    public Task<List<CustomerPointTransaction>> GetOrderRedemptionsAsync(int orderId, int customerId, CancellationToken ct)
        => _db.CustomerPointTransactions.Where(t => t.OrderId == orderId && t.CustomerId == customerId && t.TransactionType == 3)
            .OrderBy(t => t.Id).ToListAsync(ct);

    public Task<bool> HasRefundForRedemptionAsync(long sourceTransactionId, CancellationToken ct)
        => _db.CustomerPointTransactions.AsNoTracking().AnyAsync(t => t.TransactionType == 4 &&
            t.SourceTransactionId == sourceTransactionId, ct);

    public async Task AddLotAsync(CustomerPointLot lot, CancellationToken ct)
        => await _db.CustomerPointLots.AddAsync(lot, ct);

    public async Task AddTransactionsAsync(IEnumerable<CustomerPointTransaction> transactions, CancellationToken ct)
        => await _db.CustomerPointTransactions.AddRangeAsync(transactions, ct);
}
