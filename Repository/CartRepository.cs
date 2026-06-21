using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Cart;
using Shared.Models.Dtos.Cart;

namespace Repository;


public sealed class CartRepository : GenericRepository<Cart>, ICartRepository
{
    private readonly RepositoryContext _db;

    public CartRepository(RepositoryContext db) : base(db) => _db = db;

    public Task<Cart?> GetActiveByCustomerAsync(int customerId, CancellationToken ct)
    {
        // Active cart for this customer (unique by UX_Carts_CustomerId_Active)
        return _db.Carts
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Status == (byte)CartStatus.Active, ct);
    }

    public Task<CartItem?> GetByCartProductUnitAsync(int cartId, int productId, UnitLevel unitLevel, CancellationToken ct)
    {
        // One line per product+unit level (unique by UX_CartItems_CartId_ProductId_UnitLevel)
        return _db.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cartId &&
                x.ProductId == productId &&
                x.UnitLevel == (byte)unitLevel, ct);
    }


    public async Task<CartViewDto> GetMyCartAsync(int customerId, CancellationToken ct)
    {
        // 1) Load active cart header (read-only)
        var cart = await _db.Carts
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId && c.Status == 1) // 1 = Active
            .Select(c => new
            {
                c.Id,
                c.CustomerId,
                c.StoreId,
                c.Status,
                c.StatusUpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        // 2) If no active cart, return empty result
        if (cart is null)
        {
            return new CartViewDto
            {
                CartId = null,
                CustomerId = customerId,
                StoreId = null,
                Status = null,
                StatusUpdatedAt = null,
                SubtotalCurrent = 0,
                Items = new List<CartItemViewDto>()
            };
        }

        // 3) Load cart items with product data + primary image (IsPrimary = true)
        var items = await _db.CartItems
            .AsNoTracking()
            .Where(i => i.CartId == cart.Id)
            .Join(_db.Products.AsNoTracking(),
                i => i.ProductId,
                p => p.Id,
                (i, p) => new { i, p })
            .Select(x => new CartItemViewDto
            {
                CartItemId = x.i.Id,
                ProductId = x.i.ProductId,
                UnitLevel = x.i.UnitLevel,
                Quantity = x.i.Quantity,

                NameAr = x.p.NameAr,
                NameEn = x.p.NameEn,

                // Primary image url (null if not found)
                PrimaryImageUrl = _db.ProductImages
                    .AsNoTracking()
                    .Where(pi => pi.ProductId == x.p.Id && pi.IsPrimary)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault(),

                // Current unit price from product (latest)
                CurrentUnitPrice = x.i.UnitLevel == 1
                    ? x.p.OuterUnitPrice
                    : x.p.InnerUnitPrice, // may be null if inner not supported

                // Snapshot saved in cart item
                CurrentUnitPriceSnapshot = x.i.CurrentUnitPriceSnapshot,

                // Available qty in the same unit level
                AvailableQty = x.i.UnitLevel == 1
                    ? x.p.Quantity
                    : (x.p.InnerPerOuter.HasValue ? x.p.Quantity * x.p.InnerPerOuter.Value : 0),

                // Exceeds available qty (warning only; we do not block)
                ExceedsAvailableQty = x.i.UnitLevel == 1
                    ? (x.i.Quantity > x.p.Quantity)
                    : (x.p.InnerPerOuter.HasValue ? (x.i.Quantity > x.p.Quantity * x.p.InnerPerOuter.Value) : true),

                IsRedeemableByPoints = x.p.IsRedeemableByPoints,

                // Product status
                IsActive = x.p.IsActive,
                IsAvailable = x.p.IsAvailable,
                DeletedAt = x.p.DeletedAt,
                
                // Stored validity (read-only)
                IsValid = x.i.IsValid,
                InvalidReason = x.i.InvalidReason
            })
            .ToListAsync(ct);

        // 4) Compute subtotal using current product price (latest)
        // Note: if CurrentUnitPrice is null (inner price missing), treat as 0
        var subtotal = items.Sum(i => i.Quantity * (i.CurrentUnitPrice ?? 0m));

        // 5) Build response DTO
        return new CartViewDto
        {
            CartId = cart.Id,
            CustomerId = cart.CustomerId,
            StoreId = cart.StoreId,
            Status = cart.Status,
            StatusUpdatedAt = cart.StatusUpdatedAt,
            SubtotalCurrent = subtotal,
            Items = items
        };
    }


    public async Task<CartItem?> GetCartItemForUpdateAsync(int cartItemId, int customerId, CancellationToken ct)
    {
        // 1) Load cart item + cart to validate ownership
        // Future: add RowVersion concurrency to prevent lost updates
        return await _db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i =>
                i.Id == cartItemId &&
                i.Cart.CustomerId == customerId &&
                i.Cart.Status == 1, // 1=Active (MVP)
                ct);
        // Future improvements:
        // - Add query optimization (projection) if you later return rich response
        // - Support multi-store carts or cart selection per store
        // Future improvements:
        // - Allow deletion from Invalid cart too (if you decide Invalid is still editable)
        // - Add StoreId validation if needed
    }

    public void RemoveCartItem(CartItem item)
    {
        // 2) Remove item from DbSet
        _db.CartItems.Remove(item);

        // Future improvements:
        // - Use soft delete if you later need audit/history for cart actions
    }



    public async Task<Cart?> GetLatestCartByCustomerAsync(int customerId, CancellationToken ct)
    {
        // 1) Get latest cart for this customer (any status)
        // Future improvement: add a "CurrentCartId" pointer table if needed
        return await _db.Carts
            .OrderByDescending(c => c.StatusUpdatedAt)
            .ThenByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
    }


    public async Task<List<CartItem>> GetCartItemsByCartIdAsync(int cartId, CancellationToken ct)
    {
        // 2) Load all items for this cart (tracked, because we will delete)
        // Future improvement: use ExecuteDeleteAsync (EF Core 7+) to delete without loading
        return await _db.CartItems
            .Where(i => i.CartId == cartId)
            .ToListAsync(ct);
    }

    public void RemoveCartItemsRange(IEnumerable<CartItem> items)
    {
        // 3) Hard delete cart items
        _db.CartItems.RemoveRange(items);

        // Future improvement: switch to soft delete if you later need audit/history
    }

    // prview

    public async Task<CartPreviewData?> GetActiveCartDataForPreviewAsync(int cartId, int customerId, CancellationToken ct)
    {
        // 1) Load active cart header (must be Active)
        var cart = await _db.Carts
            .AsNoTracking()
            .Where(c => c.Id == cartId && c.CustomerId == customerId && c.Status == 1)
            .Select(c => new CartPreviewData
            {
                CartId = c.Id,
                Status = c.Status
            })
            .FirstOrDefaultAsync(ct);

        if (cart is null)
            return null;

        // 2) Load items + product data + primary image + group promotion membership (hook)
        var now = DateTime.UtcNow;

        cart.Items = await (
            from ci in _db.CartItems.AsNoTracking()
            join p in _db.Products.AsNoTracking() on ci.ProductId equals p.Id
            where ci.CartId == cartId
            select new CartPreviewItemData
            {
                CartItemId = ci.Id,
                ProductId = ci.ProductId,
                UnitLevel = ci.UnitLevel,
                Quantity = ci.Quantity,
                CurrentUnitPriceSnapshot = ci.CurrentUnitPriceSnapshot,

                NameAr = p.NameAr,
                NameEn = p.NameEn,

                PrimaryImageUrl = _db.ProductImages
                    .AsNoTracking()
                    .Where(pi => pi.ProductId == p.Id && pi.IsPrimary)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault(),

                OuterUnitPrice = p.OuterUnitPrice,
                InnerUnitPrice = p.InnerUnitPrice,
                InnerPerOuter = p.InnerPerOuter,

                StockOuterQty = p.Quantity,

                IsActive = p.IsActive,
                IsAvailable = p.IsAvailable,
                DeletedAt = p.DeletedAt,

                HasPromotion = p.HasPromotion,
                PromotionDiscountPercent = p.PromotionDiscountPercent,
                PromotionStartsAt = p.PromotionStartsAt,
                PromotionEndsAt = p.PromotionEndsAt,

                Points = p.Points,
                AllowSplitSale = p.AllowSplitSale,

                // Hook: set true if product is in any active promotion group (basic membership check)
                IsInGroupPromotion =
                    _db.PromotionProducts.AsNoTracking()
                        .Any(pp =>
                            pp.ProductId == p.Id &&
                            pp.Promotion.IsActive &&
                            pp.Promotion.StartAt <= now &&
                            pp.Promotion.EndAt >= now)
            }
        ).ToListAsync(ct);

        return cart;

        // Future improvements:
        // - Replace image subquery with GroupJoin for better performance
        // - Materialize group promotion membership in one query instead of Any() per row
    }

    public async Task<int> RemoveCartItemsByIdsAsync(int cartId, List<int> cartItemIds, CancellationToken ct)
    {
        // 1) Load items to delete (simple MVP)
        var items = await _db.CartItems
            .Where(x => x.CartId == cartId && cartItemIds.Contains(x.Id))
            .ToListAsync(ct);

        if (items.Count == 0)
            return 0;

        // 2) Hard delete
        _db.CartItems.RemoveRange(items);
        return items.Count;

        // Future improvements:
        // - Use ExecuteDeleteAsync (EF Core 7+) to delete without loading
    }

    public Task<bool> AddressExistsAsync(int addressId, int customerId, CancellationToken ct)
    {
        // 1) Validate address ownership (simple)
        return _db.CustomerAddresses
            .AsNoTracking()
            .AnyAsync(a => a.Id == addressId && a.CustomerId == customerId, ct);

        // Future improvements:
        // - Add address status checks (deleted/disabled)
    }



    public async Task<CartItem?> GetByCartProductUnitSourceAsync(
    int cartId,
    int productId,
    UnitLevel unitLevel,
    CartItemSourceType sourceType,
    CancellationToken ct)
    {
        // 1) Load cart item by product, unit, and source
        return await _db.CartItems
            .FirstOrDefaultAsync(i =>
                i.CartId == cartId &&
                i.ProductId == productId &&
                i.UnitLevel == (byte)unitLevel &&
                i.SourceType == (byte)sourceType,
                ct);

        // Future improvement: update unique index to include SourceType if mixing becomes allowed
    }

    public async Task<bool> HasDifferentSourceTypeAsync(
    int cartId,
    CartItemSourceType sourceType,
    CancellationToken ct)
    {
        // 1) Check if cart has items from another source type
        return await _db.CartItems
            .AsNoTracking()
            .AnyAsync(i =>
                i.CartId == cartId &&
                i.SourceType != (byte)sourceType,
                ct);

        // Future improvement: return the existing source type for better error messages
    }

    public async Task AddCartItemAsync(CartItem item, CancellationToken ct)
    {
        // 1) Add cart item
        await _db.CartItems.AddAsync(item, ct);

        // Future improvement: use bulk insert if needed
    }

    public void UpdateCartItem(CartItem item)
    {
        // 1) Update cart item
        _db.CartItems.Update(item);

        // Future improvement: use tracked entity only without explicit Update if loaded tracked
    }


    public async Task<bool> IsProductInActiveGroupPromotionAsync(int productId, CancellationToken ct)
    {
        // 1) Check if product belongs to any active group promotion
        var now = DateTime.UtcNow;

        return await (
            from pp in _db.PromotionProducts.AsNoTracking()
            join p in _db.Promotions.AsNoTracking() on pp.PromotionId equals p.Id
            where pp.ProductId == productId
                  && pp.DeletedAt == null
                  && p.DeletedAt == null
                  && p.IsActive
                  && p.StartAt != null
                  && p.EndAt != null
                  && p.StartAt <= now
                  && p.EndAt >= now
            select pp.Id
        ).AnyAsync(ct);

        // Future improvement: return promotion id if UI needs it
    }


    public async Task AddCartAsync(Cart cart, CancellationToken ct)
    {
        // 1) Add cart
        await _db.Carts.AddAsync(cart, ct);

        // Future improvement: handle unique active cart conflict here
    }

}
