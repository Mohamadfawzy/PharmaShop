using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Order;
using Shared.Models.Dtos.Order.order;
using Shared.Responses;

namespace Repository;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly RepositoryContext _db;

    public OrderRepository(RepositoryContext db) : base(db)
    {
        _db = db;
    }

    public async Task AddOrderAsync(Order order, CancellationToken ct)
    {
        // 1) Add order to DbContext (no save here)
        await _db.Orders.AddAsync(order, ct);

        // Future improvement: validate required fields before add
    }

    public async Task AddItemsRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct)
    {
        // 1) Add order items to DbContext (no save here)
        await _db.OrderItems.AddRangeAsync(items, ct);

        // Future improvement: consider bulk insert for large orders
    }

    public async Task<List<CheckoutLineData>> GetLinesFromCartAsync(int cartId,int customerId,CancellationToken ct)
    {
        // 1) Load normal cart lines only
        return await (
            from c in _db.Carts.AsNoTracking()
            join ci in _db.CartItems.AsNoTracking() on c.Id equals ci.CartId
            join p in _db.Products.AsNoTracking() on ci.ProductId equals p.Id
            where c.Id == cartId
                  && c.CustomerId == customerId
                  && c.Status == 1
                  && ci.SourceType == 1 // Normal only
            select new CheckoutLineData
            {
                CartId = c.Id,
                StoreId = c.StoreId,
                PrescriptionId = null,

                ProductId = ci.ProductId,
                UnitLevel = ci.UnitLevel,
                Quantity = ci.Quantity,

                OuterUnitPrice = p.OuterUnitPrice,
                InnerUnitPrice = p.InnerUnitPrice,

                OuterUnitId = p.OuterUnitId,
                InnerUnitId = p.InnerUnitId,
                InnerPerOuter = p.InnerPerOuter,

                AllowSplitSale = p.AllowSplitSale,
                Points = p.Points,

                IsRedeemableByPoints = p.IsRedeemableByPoints,
                CartItemSourceType = ci.SourceType
            }
        ).ToListAsync(ct);

        // Future improvement: validate product availability here if needed
    }

    public async Task<List<CheckoutLineData>> GetLinesFromPrescriptionAsync(int prescriptionId, int customerId, CancellationToken ct)
    {
        // 1) Load prescription lines (prescription must be ReadyForCheckout and owned by customer)
        return await (
            from pr in _db.Prescriptions.AsNoTracking()
            join pi in _db.PrescriptionItems.AsNoTracking() on pr.Id equals pi.PrescriptionId
            join p in _db.Products.AsNoTracking() on pi.ProductId equals p.Id
            where pr.Id == prescriptionId
                  && pr.CustomerId == customerId
                  && pr.Status == 3                      // 3=ReadyForCheckout
                  && pi.ProductId != null
            select new CheckoutLineData
            {
                PrescriptionId = pr.Id,
                StoreId = pr.StoreId,
                CartId = null,

                ProductId = p.Id,
                UnitLevel = 1,                           // MVP: prescription checkout uses Outer only
                Quantity = (pi.RequestedQuantity ?? 1m),

                OuterUnitPrice = p.OuterUnitPrice,
                InnerUnitPrice = p.InnerUnitPrice,

                OuterUnitId = p.OuterUnitId,
                InnerUnitId = p.InnerUnitId,
                InnerPerOuter = p.InnerPerOuter,

                AllowSplitSale = p.AllowSplitSale,
                Points = p.Points
            }
        ).ToListAsync(ct);

        // Future improvement: allow pharmacist to set unit level per item
    }

    public async Task<List<CheckoutLineData>> GetLinesFromPointsRedemptionCartAsync(int cartId, int customerId, CancellationToken ct)
    {
        // 1) Load points redemption cart lines only
        return await (
            from c in _db.Carts.AsNoTracking()
            join ci in _db.CartItems.AsNoTracking() on c.Id equals ci.CartId
            join p in _db.Products.AsNoTracking() on ci.ProductId equals p.Id
            where c.Id == cartId
                  && c.CustomerId == customerId
                  && c.Status == 1
                  && ci.SourceType == 2 // PointsRedemption only
            select new CheckoutLineData
            {
                CartId = c.Id,
                StoreId = c.StoreId,
                PrescriptionId = null,

                ProductId = ci.ProductId,
                UnitLevel = ci.UnitLevel,
                Quantity = ci.Quantity,

                OuterUnitPrice = p.OuterUnitPrice,
                InnerUnitPrice = p.InnerUnitPrice,

                OuterUnitId = p.OuterUnitId,
                InnerUnitId = p.InnerUnitId,
                InnerPerOuter = p.InnerPerOuter,

                AllowSplitSale = p.AllowSplitSale,
                Points = p.Points,

                IsRedeemableByPoints = p.IsRedeemableByPoints,
                CartItemSourceType = ci.SourceType
            }
        ).ToListAsync(ct);

        // Future improvement: block products with active promotions here if needed
    }

    public async Task<(bool ok, CustomerAddressSnapshot snap)> GetAddressSnapshotAsync(int addressId, int customerId, CancellationToken ct)
    {
        // 1) Load address and validate ownership
        var addr = await _db.CustomerAddresses.AsNoTracking()
            .Where(a => a.Id == addressId && a.CustomerId == customerId)
            .Select(a => new CustomerAddressSnapshot
            {
                AddressId = a.Id,
                Title = a.Title,
                City = a.City,
                Region = a.Region,
                Street = a.Street,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                //Phone = a.phone
            })
            .FirstOrDefaultAsync(ct);

        return (addr != null, addr ?? new CustomerAddressSnapshot());
    }

    public async Task<PagedResult<AdminOrderListItemDto>> SearchAdminAsync( AdminOrderListQueryDto q, CancellationToken ct)
    {
        // 1) Base query
        IQueryable<Order> query = _db.Orders.AsNoTracking();

        // 2) Required Store filter
        query = query.Where(o => o.StoreId == q.StoreId);

        // 3) Optional filters
        if (q.Status.HasValue)
            query = query.Where(o => o.Status == q.Status.Value);

        if (q.PaymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == q.PaymentStatus.Value);

        if (q.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == q.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(q.OrderNumber))
            query = query.Where(o => o.OrderNumber.Contains(q.OrderNumber));

        if (q.From.HasValue)
            query = query.Where(o => o.CreatedAt >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(o => o.CreatedAt <= q.To.Value);

        // 4) Count total
        var total = await query.CountAsync(ct);

        // 5) Sorting (latest status update)
        query = query.OrderByDescending(o => o.StatusUpdatedAt);

        // 6) Pagination
        var skip = (q.Page - 1) * q.PageSize;

        var items = await query
            .Skip(skip)
            .Take(q.PageSize)
            .Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                StoreId = o.StoreId,
                Status = o.Status,
                StatusUpdatedAt = o.StatusUpdatedAt,
                PaymentStatus = o.PaymentStatus,
                GrandTotal = o.GrandTotal,
                PrescriptionId = o.PrescriptionId,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

        // 7) Return paged result
        return new PagedResult<AdminOrderListItemDto>
        {
            Items = items,
            TotalCount = total
        };

        // Future improvements:
        // - Add join with Customer to return name/phone
        // - Add index-based filtering optimizations
    }

    //================== 2
    public async Task<Dictionary<int, ActiveGroupPromotionHit>> GetActiveGroupPromotionsByProductAsync(List<int> productIds, CancellationToken ct)
    {
        // 1) Load active group promotions for products (choose highest discount per product)
        var now = DateTime.UtcNow;

        var rows = await (
            from pp in _db.PromotionProducts.AsNoTracking()
            join pr in _db.Promotions.AsNoTracking() on pp.PromotionId equals pr.Id
            where productIds.Contains(pp.ProductId!.Value)
                  && pp.DeletedAt == null
                  && pr.DeletedAt == null
                  && pr.IsActive
                  && pr.StartAt != null && pr.EndAt != null
                  && pr.StartAt <= now && pr.EndAt >= now
            select new
            {
                ProductId = pp.ProductId!.Value,
                PromotionId = pr.Id,
                pr.BasicAmount,
                pr.OfferAmount,
                pr.DiscountPercent
            }
        ).ToListAsync(ct);

        // 2) Pick best promotion per product (MVP rule)
        return rows
            .Where(x => x.BasicAmount.HasValue && x.OfferAmount.HasValue && x.DiscountPercent.HasValue)
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var best = g.OrderByDescending(x => x.DiscountPercent!.Value).First();
                    return new ActiveGroupPromotionHit
                    {
                        PromotionId = best.PromotionId,
                        BasicAmount = best.BasicAmount!.Value,
                        OfferAmount = best.OfferAmount!.Value,
                        DiscountPercent = best.DiscountPercent!.Value
                    };
                });

        // Future improvement: support multiple group promotions and choose best for entire cart
    }

    public async Task<Dictionary<int, decimal>> GetActiveProductPromotionPercentAsync(List<int> productIds, CancellationToken ct)
    {
        // 1) Load product-level promotions from Products table (active window only)
        var now = DateTime.UtcNow;

        var rows = await _db.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id)
                        && p.HasPromotion
                        && p.PromotionStartsAt != null
                        && p.PromotionEndsAt != null
                        && p.PromotionStartsAt <= now
                        && p.PromotionEndsAt >= now)
            .Select(p => new { p.Id, p.PromotionDiscountPercent })
            .ToListAsync(ct);

        return rows.ToDictionary(x => x.Id, x => x.PromotionDiscountPercent);

        // Future improvement: merge with promotions table if product promos move out of Products
    }

    // ==========================3

    public async Task MarkCartCheckedOutAsync(int cartId, CancellationToken ct)
    {
        // 1) Update cart status to CheckedOut (2)
        await _db.Carts.Where(c => c.Id == cartId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Status, (byte)2)
                .SetProperty(c => c.StatusUpdatedAt, DateTime.UtcNow)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                ct);

        // Future improvement: also archive cart items if needed
    }

    public async Task ClosePrescriptionAsync(int prescriptionId, CancellationToken ct)
    {
        // 1) Update prescription status to Closed (4)
        await _db.Prescriptions.Where(p => p.Id == prescriptionId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, (byte)4)
                .SetProperty(p => p.StatusUpdatedAt, DateTime.UtcNow)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
                ct);

        // Future improvement: prevent closing if no order created
    }

    public async Task<int> GetCustomerPointsAsync(int customerId, CancellationToken ct)
    {
        // 1) Read fast points balance
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => c.Points)
            .FirstOrDefaultAsync(ct);

        // Future improvement: compare periodically with lots sum for reconciliation
    }

    public async Task<List<CustomerPointLot>> GetAvailablePointLotsForUpdateAsync(int customerId,DateTime nowUtc,CancellationToken ct)
    {
        // 1) Load available point lots using FEFO order
        return await _db.CustomerPointLots
            .Where(x =>
                x.CustomerId == customerId &&
                x.Status == 2 &&
                x.RemainingPoints > 0 &&
                x.ExpiresAt > nowUtc)
            .OrderBy(x => x.ExpiresAt)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        // Future improvement: use UPDLOCK query for stronger SQL Server concurrency protection
    }

    public async Task AddPointTransactionsRangeAsync(
    IEnumerable<CustomerPointTransaction> transactions,
    CancellationToken ct)
    {
        // 1) Add point ledger transactions
        await _db.CustomerPointTransactions.AddRangeAsync(transactions, ct);

        // Future improvement: add idempotency key per order
    }


    public async Task AddCustomerPointLotAsync(CustomerPointLot lot, CancellationToken ct)
    {
        // 1) Add customer point lot
        await _db.CustomerPointLots.AddAsync(lot, ct);

        // Future improvement: split lots by order item if needed
    }

    public async Task<int> DecreaseCustomerPointsAsync(int customerId, int points, CancellationToken ct)
    {
        // 1) Decrease customer points safely
        return await _db.Customers
            .Where(c => c.Id == customerId && c.Points >= points)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Points, c => c.Points - points),
                ct);

        // Future improvement: use rowversion on Customers for concurrency
    }

    public async Task<HashSet<byte>> GetCartItemSourceTypesAsync(int cartId,int customerId,CancellationToken ct)
    {
        // 1) Load distinct cart item source types
        var sourceTypes = await (
            from c in _db.Carts.AsNoTracking()
            join ci in _db.CartItems.AsNoTracking() on c.Id equals ci.CartId
            where c.Id == cartId
                  && c.CustomerId == customerId
                  && c.Status == 1
            select ci.SourceType
        )
        .Distinct()
        .ToListAsync(ct);

        return sourceTypes.ToHashSet();

        // Future improvement: return cart status too for better messages
    }

    // order status

    public async Task<Order?> GetOrderForStatusUpdateAsync(int orderId, CancellationToken ct)
    {
        // 1) Load order for update (tracked)
        return await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        // Future improvement: add StoreId scope for admin authorization
    }

    public async Task<List<CustomerPointLot>> GetPendingPointLotsByOrderAsync(int orderId, CancellationToken ct)
    {
        // 1) Load pending point lots for this order (tracked)
        return await _db.CustomerPointLots
            .Where(x =>
                x.OrderId == orderId &&
                x.Status == 1 &&              // 1=Pending
                x.RemainingPoints > 0)
            .ToListAsync(ct);

        // Future improvement: add CustomerId filter for extra safety
    }

    public async Task<Customer?> GetCustomerForPointsUpdateAsync(int customerId, CancellationToken ct)
    {
        // 1) Load customer for points update (tracked)
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);

        // Future improvement: use rowversion/concurrency token on Customers
    }

    public void UpdateOrder(Order order)
    {
        // 1) Mark order as updated
        _db.Orders.Update(order);

        // Future improvement: tracked entity may not need explicit Update
    }


}
