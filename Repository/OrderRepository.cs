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
                CartItemSourceType = ci.SourceType,
                IsProductActive = p.IsActive && p.DeletedAt == null,
                IsProductAvailable = p.IsAvailable,
                AvailableOuterQuantity = p.Quantity
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
                  && p.StoreId == pr.StoreId
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
                Points = p.Points,
                IsProductActive = p.IsActive && p.DeletedAt == null,
                IsProductAvailable = p.IsAvailable,
                AvailableOuterQuantity = p.Quantity
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
                CartItemSourceType = ci.SourceType,
                IsProductActive = p.IsActive && p.DeletedAt == null,
                IsProductAvailable = p.IsAvailable,
                AvailableOuterQuantity = p.Quantity
            }
        ).ToListAsync(ct);

        // Future improvement: block products with active promotions here if needed
    }

    public async Task<(bool ok, CustomerAddressSnapshot snap)> GetAddressSnapshotAsync(int addressId, int customerId, CancellationToken ct)
    {
        // 1) Load address and validate ownership
        var addr = await _db.CustomerAddresses.AsNoTracking()
            .Where(a => a.Id == addressId && a.CustomerId == customerId && a.DeletedAt == null)
            .Select(a => new CustomerAddressSnapshot
            {
                AddressId = a.Id,
                GovernorateAreaId = a.GovernorateAreaId,
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

        // 2) Optional store filter
        if (q.StoreId.HasValue) query = query.Where(o => o.StoreId == q.StoreId.Value);

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

    public async Task<PagedResult<CustomerOrderListItemDto>> SearchCustomerAsync(int customerId, CustomerOrderListQueryDto q, CancellationToken ct)
    {
        IQueryable<Order> query = _db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId);
        if (q.Status.HasValue) query = query.Where(o => o.Status == q.Status.Value);
        var total = await query.CountAsync(ct);
        query = q.NewestFirst ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt);
        var items = await query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(o => new CustomerOrderListItemDto
            {
                Id = o.Id, OrderNumber = o.OrderNumber, StoreId = o.StoreId, OrderSource = o.OrderSource,
                Status = o.Status, StatusUpdatedAt = o.StatusUpdatedAt, PaymentStatus = o.PaymentStatus,
                GrandTotal = o.GrandTotal, CreatedAt = o.CreatedAt
            }).ToListAsync(ct);
        return new PagedResult<CustomerOrderListItemDto> { Items = items, TotalCount = total };
    }

    public Task<CustomerOrderDetailsDto?> GetCustomerOrderDetailsAsync(int orderId, int customerId, CancellationToken ct)
        => _db.Orders.AsNoTracking().Where(o => o.Id == orderId && o.CustomerId == customerId)
            .Select(o => new CustomerOrderDetailsDto
            {
                Id = o.Id, OrderNumber = o.OrderNumber, StoreId = o.StoreId, OrderSource = o.OrderSource,
                Status = o.Status, StatusUpdatedAt = o.StatusUpdatedAt, PaymentStatus = o.PaymentStatus,
                GrandTotal = o.GrandTotal, CreatedAt = o.CreatedAt, PaymentMethod = o.PaymentMethod,
                Subtotal = o.Subtotal, ItemsDiscountTotal = o.ItemsDiscountTotal, DeliveryFee = o.DeliveryFee,
                RedeemedPoints = o.RedeemedPoints, RedeemedAmount = o.RedeemedAmount, EarnedPoints = o.EarnedPoints,
                PrescriptionId = o.PrescriptionId, DeliveryTitle = o.DeliveryTitle, DeliveryCity = o.DeliveryCity,
                DeliveryRegion = o.DeliveryRegion, DeliveryStreet = o.DeliveryStreet, DeliveryLatitude = o.DeliveryLatitude,
                DeliveryLongitude = o.DeliveryLongitude, DeliveryPhone = o.DeliveryPhone, Notes = o.Notes,
                Items = o.OrderItems.OrderBy(i => i.Id).Select(i => new OrderItemSnapshotDto
                {
                    Id = i.Id, ProductId = i.ProductId, ProductNameAr = i.Product.NameAr, ProductNameEn = i.Product.NameEn,
                    UnitLevel = i.UnitLevel, Quantity = i.Quantity, OuterUnitIdSnapshot = i.OuterUnitIdSnapshot,
                    InnerUnitIdSnapshot = i.InnerUnitIdSnapshot, InnerPerOuterSnapshot = i.InnerPerOuterSnapshot,
                    UnitPriceSnapshot = i.UnitPriceSnapshot, DiscountPercentSnapshot = i.DiscountPercentSnapshot,
                    FinalUnitPriceSnapshot = i.FinalUnitPriceSnapshot, LineTotal = i.LineTotal,
                    PointsSnapshot = i.PointsSnapshot, EarnedPoints = i.EarnedPoints, AppliedPromotionId = i.AppliedPromotionId
                }).ToList()
            }).FirstOrDefaultAsync(ct);

    public Task<AdminOrderDetailsDto?> GetAdminOrderDetailsAsync(int orderId, CancellationToken ct)
        => _db.Orders.AsNoTracking().Where(o => o.Id == orderId)
            .Select(o => new AdminOrderDetailsDto
            {
                Id = o.Id, OrderNumber = o.OrderNumber, StoreId = o.StoreId, OrderSource = o.OrderSource,
                Status = o.Status, StatusUpdatedAt = o.StatusUpdatedAt, PaymentStatus = o.PaymentStatus,
                GrandTotal = o.GrandTotal, CreatedAt = o.CreatedAt, PaymentMethod = o.PaymentMethod,
                Subtotal = o.Subtotal, ItemsDiscountTotal = o.ItemsDiscountTotal, DeliveryFee = o.DeliveryFee,
                RedeemedPoints = o.RedeemedPoints, RedeemedAmount = o.RedeemedAmount, EarnedPoints = o.EarnedPoints,
                PrescriptionId = o.PrescriptionId, DeliveryTitle = o.DeliveryTitle, DeliveryCity = o.DeliveryCity,
                DeliveryRegion = o.DeliveryRegion, DeliveryStreet = o.DeliveryStreet, DeliveryLatitude = o.DeliveryLatitude,
                DeliveryLongitude = o.DeliveryLongitude, DeliveryPhone = o.DeliveryPhone, Notes = o.Notes,
                CustomerId = o.CustomerId, CustomerNameAr = o.Customer.FullNameAr, CustomerPhone = o.Customer.PhoneNumber,
                Items = o.OrderItems.OrderBy(i => i.Id).Select(i => new OrderItemSnapshotDto
                {
                    Id = i.Id, ProductId = i.ProductId, ProductNameAr = i.Product.NameAr, ProductNameEn = i.Product.NameEn,
                    UnitLevel = i.UnitLevel, Quantity = i.Quantity, OuterUnitIdSnapshot = i.OuterUnitIdSnapshot,
                    InnerUnitIdSnapshot = i.InnerUnitIdSnapshot, InnerPerOuterSnapshot = i.InnerPerOuterSnapshot,
                    UnitPriceSnapshot = i.UnitPriceSnapshot, DiscountPercentSnapshot = i.DiscountPercentSnapshot,
                    FinalUnitPriceSnapshot = i.FinalUnitPriceSnapshot, LineTotal = i.LineTotal,
                    PointsSnapshot = i.PointsSnapshot, EarnedPoints = i.EarnedPoints, AppliedPromotionId = i.AppliedPromotionId
                }).ToList()
            }).FirstOrDefaultAsync(ct);

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

    public async Task<int> MarkCartCheckedOutAsync(int cartId, int customerId, CancellationToken ct)
    {
        // Compare-and-set prevents two concurrent checkouts from consuming the same cart.
        return await _db.Carts.Where(c => c.Id == cartId && c.CustomerId == customerId && c.Status == 1)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Status, (byte)2)
                .SetProperty(c => c.StatusUpdatedAt, DateTime.UtcNow)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                ct);
    }

    public async Task<int> ClosePrescriptionAsync(int prescriptionId, int customerId, CancellationToken ct)
    {
        // Compare-and-set ensures only a ready prescription can be checked out once.
        return await _db.Prescriptions.Where(p => p.Id == prescriptionId && p.CustomerId == customerId && p.Status == 3)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, (byte)4)
                .SetProperty(p => p.StatusUpdatedAt, DateTime.UtcNow)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
                ct);

        // Future improvement: prevent closing if no order created
    }

    public async Task<int> GetCustomerPointsAsync(int customerId, CancellationToken ct)
    {
        // Redeemable balance excludes expired lots even if the scheduled sweep has not run yet.
        return await _db.CustomerPointLots
            .AsNoTracking()
            .Where(l => l.CustomerId == customerId && l.Status == 2 && l.RemainingPoints > 0 && l.ExpiresAt > DateTime.UtcNow)
            .SumAsync(l => (int?)l.RemainingPoints, ct) ?? 0;
    }

    public async Task<List<CustomerPointLot>> GetAvailablePointLotsForUpdateAsync(int customerId,DateTime nowUtc,CancellationToken ct)
    {
        // Lock FEFO lots while redeeming so concurrent checkouts cannot consume the same balance.
        return await _db.CustomerPointLots
            .FromSqlInterpolated($"SELECT * FROM [dbo].[CustomerPointLots] WITH (UPDLOCK, ROWLOCK, READPAST) WHERE [CustomerId] = {customerId} AND [Status] = 2 AND [RemainingPoints] > 0 AND [ExpiresAt] > {nowUtc}")
            .OrderBy(x => x.ExpiresAt).ThenBy(x => x.Id).ToListAsync(ct);
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
