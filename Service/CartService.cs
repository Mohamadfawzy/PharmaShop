using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Cart;
using Shared.Models.Dtos.Cart;
using Shared.Responses;

namespace Service;

public class CartService : ICartService
{
    private readonly IUnitOfWork unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }


    public async Task<AppResponse<AddToCartResponseDto>> AddItemAsync(CartAddItemDto dto, CancellationToken ct)
    {
        // 1) Basic validation
        if (dto.CustomerId <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["CustomerId"] = new[] { "CustomerId is required" } },
                detail: "Validation failed"
            );

        if (dto.StoreId <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["StoreId"] = new[] { "StoreId is required" } },
                detail: "Validation failed"
            );

        if (dto.ProductId <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["ProductId"] = new[] { "ProductId is required" } },
                detail: "Validation failed"
            );

        if (dto.Quantity <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["Quantity"] = new[] { "Quantity must be greater than 0" } },
                detail: "Validation failed"
            );

        // 2) Load product (current state)
        var product = await unitOfWork.Products.GetByIdNoTrackingAsync(dto.ProductId, ct);
        if (product is null)
        {
            return AppResponse<AddToCartResponseDto>.NotFound("Product not found");
        }

        // 3) Validate unit level rules (outer/inner)
        if (dto.UnitLevel == UnitLevel.Inner)
        {
            // Inner sale must be allowed and have inner price
            if (!product.AllowSplitSale || product.InnerUnitPrice is null || product.InnerPerOuter is null || product.InnerPerOuter < 1)
            {
                return AppResponse<AddToCartResponseDto>.BusinessRuleViolation(
                    "Inner unit sale is not allowed for this product"
                );
            }
        }

        // 4) Compute current unit price based on UnitLevel
        var currentUnitPrice = dto.UnitLevel == UnitLevel.Outer
            ? product.OuterUnitPrice
            : product.InnerUnitPrice!.Value;

        // 5) Compute available qty based on stored outer qty
        // Quantity in Products is stored in OUTER units
        var availableQty = dto.UnitLevel == UnitLevel.Outer
            ? product.Quantity
            : product.Quantity * product.InnerPerOuter!.Value;

        // 6) Get or create active cart
        // Note: We keep it simple; in rare concurrent calls, unique index may throw; we handle below.
        Cart cart;
        cart = await unitOfWork.Carts.GetActiveByCustomerAsync(dto.CustomerId, ct)
               ?? await CreateActiveCartAsync(dto, ct);

        // 7) Upsert cart item (same product + same unit level)
        var cartItem = await unitOfWork.Carts.GetByCartProductUnitAsync(cart.Id, dto.ProductId, dto.UnitLevel, ct);

        var now = DateTime.UtcNow;

        bool requiresRefresh = false;
        string? refreshReason = null;
        ItemPriceChangeDto? priceChange = null;

        if (cartItem is null)
        {
            // 7.1 Create new line
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                UnitLevel = (byte)dto.UnitLevel,
                Quantity = dto.Quantity,

                // Snapshot logic:
                // UnitPriceSnapshot = old price reference (initially same as current)
                // CurrentUnitPriceSnapshot = last acknowledged current price
                UnitPriceSnapshot = currentUnitPrice,
                CurrentUnitPriceSnapshot = currentUnitPrice,

                // Limit snapshots
                MinOrderQtySnapshot = product.MinOrderQty,
                MaxOrderQtySnapshot = product.MaxOrderQty,
                MaxPerDayQtySnapshot = product.MaxPerDayQty,

                // Validity flags (product can be added, but may block next step)
                IsValid = true,
                InvalidReason = null,

                CreatedAt = now
            };

            // If product is inactive/unavailable/deleted => mark invalid and block next step
            MarkInvalidIfNeeded(cart, cartItem, product, now);

            await unitOfWork.CartItems.AddAsync(cartItem, ct);
        }
        else
        {
            // 7.2 Increase qty automatically
            var newQty = cartItem.Quantity + dto.Quantity;
            cartItem.Quantity = newQty;
            cartItem.UpdatedAt = now;

            // 7.3 Detect price change compared to last known current snapshot
            if (cartItem.CurrentUnitPriceSnapshot != currentUnitPrice)
            {
                // Move last known current price to old snapshot
                cartItem.UnitPriceSnapshot = cartItem.CurrentUnitPriceSnapshot;
                cartItem.CurrentUnitPriceSnapshot = currentUnitPrice;

                // Mark cart as invalid and require refresh before checkout
                requiresRefresh = true;
                refreshReason = "Price changed. Refresh cart before checkout.";

                priceChange = new ItemPriceChangeDto
                {
                    OldPrice = cartItem.UnitPriceSnapshot,
                    NewPrice = cartItem.CurrentUnitPriceSnapshot,
                    ChangedAtUtc = now
                };

                SetCartStatus(cart, CartStatus.Invalid, refreshReason, now);
            }

            // 7.4 Update limit snapshots to latest product values
            cartItem.MinOrderQtySnapshot = product.MinOrderQty;
            cartItem.MaxOrderQtySnapshot = product.MaxOrderQty;
            cartItem.MaxPerDayQtySnapshot = product.MaxPerDayQty;

            // 7.5 Re-check validity (product may become invalid)
            MarkInvalidIfNeeded(cart, cartItem, product, now);

            unitOfWork.CartItems.Update(cartItem);
        }

        // 8) Persist changes
        try
        {
            await unitOfWork.CompleteAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // 8.1 Handle rare concurrency for creating active cart (unique index)
            // If another request created an active cart at the same time, re-fetch it and retry once.
            // Keep it simple for MVP.
            var activeCart = await unitOfWork.Carts.GetActiveByCustomerAsync(dto.CustomerId, ct);
            if (activeCart is not null && activeCart.Id != cart.Id)
            {
                cart = activeCart;

                // Re-run minimal insert into the correct cart
                var retryItem = await unitOfWork.Carts.GetByCartProductUnitAsync(cart.Id, dto.ProductId, dto.UnitLevel, ct);
                if (retryItem is null)
                {
                    cartItem.CartId = cart.Id;
                    await unitOfWork.CartItems.AddAsync(cartItem, ct);
                }
                else
                {
                    retryItem.Quantity += dto.Quantity;
                    retryItem.UpdatedAt = now;
                    unitOfWork.CartItems.Update(retryItem);
                    cartItem = retryItem;
                }

                await unitOfWork.CompleteAsync(ct);
            }
            else
            {
                throw; // Let middleware handle unexpected DB errors
            }
        }

        // 9) Build response
        var exceeds = cartItem.Quantity > availableQty;

        var response = new AddToCartResponseDto
        {
            CartId = cart.Id,
            CartStatus = (CartStatus)cart.Status,

            CartItemId = cartItem.Id,
            ProductId = cartItem.ProductId,
            UnitLevel = (UnitLevel)cartItem.UnitLevel,
            Quantity = cartItem.Quantity,

            AvailableQty = availableQty,
            ExceedsAvailableQty = exceeds,

            RequiresRefresh = requiresRefresh || (CartStatus)cart.Status == CartStatus.Invalid,
            RefreshReason = refreshReason,

            PriceChange = priceChange
        };

        return AppResponse<AddToCartResponseDto>.Ok(response, "Item added to cart successfully");
    }



    // -------------------------
    // Get My Cart Async
    // ------------------------- 
    public async Task<AppResponse<CartViewDto>> GetMyCartAsync(int customerId, CancellationToken ct)
    {
        // 1) Validate inputs
        if (customerId <= 0)
        {
            return AppResponse<CartViewDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["customerId"] = new[] { "customerId is required" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load cart view (read-only)
        var cart = await unitOfWork.Carts.GetMyCartAsync(customerId, ct);

        // 3) Return response
        return AppResponse<CartViewDto>.Ok(cart, title: "Cart retrieved successfully");
    }



    public async Task<AppResponse<int>> UpdateItemQtyAsync(int cartItemId, CartUpdateQtyDto dto, CancellationToken ct)
    {
        // 1) Basic validation
        if (cartItemId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["cartItemId"] = new[] { "Invalid cartItemId" }
                },
                detail: "Validation failed"
            );
        }

        if (dto.CustomerId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["CustomerId"] = new[] { "CustomerId is required" }
                },
                detail: "Validation failed"
            );
        }

        // Quantity cannot be 0 (remove is a separate endpoint)
        if (dto.Quantity < 1)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Quantity"] = new[] { "Quantity must be at least 1. Use Remove endpoint to delete item." }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load cart item (with ownership check)
        var item = await unitOfWork.Carts.GetCartItemForUpdateAsync(cartItemId, dto.CustomerId, ct);
        if (item is null)
        {
            return AppResponse<int>.NotFound("Cart item not found");
        }

        // 3) Update qty
        // Future: enforce product Min/Max constraints here if required for MVP
        item.Quantity = dto.Quantity;
        item.UpdatedAt = DateTime.UtcNow;

        // Touch cart updated time
        // Future: set cart status/flags if you want server-side invalidation logic
        item.Cart.UpdatedAt = item.UpdatedAt;

        // 4) Save changes
        await unitOfWork.CompleteAsync(ct);

        // 5) Return success (return item id for simplicity)
        return AppResponse<int>.Ok(item.Id, "Quantity updated successfully");

        // Future improvements:
        // - Verify stock availability and return AvailableQty in response
        // - Detect price changes and set cart invalid server-side (if you later decide)
        // - Replace CustomerId in body with CustomerId from token claims
        // - Add optimistic concurrency (RowVersion) to avoid last-write-wins issues
    }



    public async Task<AppResponse<int>> RemoveItemAsync(int cartItemId, int customerId, CancellationToken ct)
    {
        // 1) Basic validation
        if (cartItemId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["cartItemId"] = new[] { "Invalid cartItemId" }
                },
                detail: "Validation failed"
            );
        }

        if (customerId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["customerId"] = new[] { "customerId is required" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load cart item with ownership check (tracked entity)
        var item = await unitOfWork.Carts.GetCartItemForUpdateAsync(cartItemId, customerId, ct);
        if (item is null)
            return AppResponse<int>.NotFound("Cart item not found");

        // 3) Remove item
        unitOfWork.Carts.RemoveCartItem(item);

        // Touch cart updated time (optional, helps tracking changes)
        item.Cart.UpdatedAt = DateTime.UtcNow;

        // 4) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 5) Return success
        return AppResponse<int>.Ok(cartItemId, "Item removed successfully");

        // Future improvements:
        // - If cart becomes empty, optionally mark cart as Abandoned or keep Active (business decision)
        // - If cart status is not Active, return a BusinessRuleViolation with proper message
        // - Add optimistic concurrency (RowVersion) to avoid race conditions
    }



    public async Task<AppResponse<int>> ClearCartAsync(int customerId, CancellationToken ct)
    {
        // 1) Validate inputs
        if (customerId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["CustomerId"] = new[] { "CustomerId is required" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load latest cart for this customer (any status)
        var cart = await unitOfWork.Carts.GetLatestCartByCustomerAsync(customerId, ct);
        if (cart is null)
            return AppResponse<int>.NotFound("No cart found for this customer");

        // 3) Load items
        var items = await unitOfWork.Carts.GetCartItemsByCartIdAsync(cart.Id, ct);

        // 4) If cart is already empty, return OK with 0
        if (items.Count == 0)
            return AppResponse<int>.Ok(0, "Nothing to clear");

        // 5) Delete all items (hard delete)
        unitOfWork.Carts.RemoveCartItemsRange(items);
        var deletedCount = items.Count;

        // 6) Update cart state after clear
        var now = DateTime.UtcNow;

        cart.Status = 3; // 3=Abandoned
        cart.StatusUpdatedAt = now;

        cart.ExpiredReason = null; // Reset status reason
        cart.DeviceId = null;      // Reset (as per your rule "reset/clear")
        cart.AppInstanceId = null; // Reset (as per your rule "reset/clear")

        cart.UpdatedAt = now;

        // 7) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 8) Return countDeleted
        return AppResponse<int>.Ok(deletedCount, "Cart cleared successfully");

        // Future improvements:
        // - If you later want to clear a specific cart by cartId instead of "latest", add parameter
        // - Use ExecuteDeleteAsync for large carts (performance)
        // - Use transaction if you later add more side effects (events/logs)
    }


    // -------------------------
    // Helpers (private methods)
    // -------------------------

    private async Task<Cart> CreateActiveCartAsync(CartAddItemDto dto, CancellationToken ct)
    {
        // Create a new active cart for customer
        var now = DateTime.UtcNow;

        var cart = new Cart
        {
            CustomerId = dto.CustomerId,
            StoreId = dto.StoreId,

            Status = (byte)CartStatus.Active,
            StatusUpdatedAt = now,
            ExpiredReason = null,

            DeviceId = string.IsNullOrWhiteSpace(dto.DeviceId) ? null : dto.DeviceId.Trim(),
            AppInstanceId = string.IsNullOrWhiteSpace(dto.AppInstanceId) ? null : dto.AppInstanceId.Trim(),

            CreatedAt = now
        };

        await unitOfWork.Carts.AddAsync(cart, ct);
        await unitOfWork.CompleteAsync(ct); // Persist to get CartId

        return cart;
    }

    private static void SetCartStatus(Cart cart, CartStatus status, string? reason, DateTime nowUtc)
    {
        // Set cart status and reason
        cart.Status = (byte)status;
        cart.StatusUpdatedAt = nowUtc;
        cart.ExpiredReason = string.IsNullOrWhiteSpace(reason) ? null : reason;
        cart.UpdatedAt = nowUtc;
    }

    private static void MarkInvalidIfNeeded(Cart cart, CartItem item, Product product, DateTime nowUtc)
    {
        // Product can remain in cart, but invalid products block next step
        var isInvalidProduct =
            product.DeletedAt != null ||
            product.IsActive == false ||
            product.IsAvailable == false;

        if (!isInvalidProduct)
        {
            item.IsValid = true;
            item.InvalidReason = null;
            return;
        }

        item.IsValid = false;
        item.InvalidReason = product.DeletedAt != null
            ? "Product was deleted"
            : (product.IsActive == false ? "Product is inactive" : "Product is unavailable");

        item.UpdatedAt = nowUtc;

        // Cart becomes invalid until user removes invalid items
        SetCartStatus(cart, CartStatus.Invalid, "Cart contains invalid items. Remove them to continue.", nowUtc);
    }

}
