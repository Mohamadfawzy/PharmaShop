using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Service.Models.Carts;
using Shared.Enums;
using Shared.Enums.Cart;
using Shared.Models.Dtos.Cart;
using Shared.Responses;

namespace Service;

public class CartService : ICartService
{
    private readonly IUnitOfWork unitOfWork;

    // Simple settings (later move to appsettings.json)
    private const int ConversionRate = 30;          // 30 points = 1 money unit
    private const int MinimumRedeemPoints = 500;    // minimum points to redeem
    private const decimal RedeemCapPercent = 0.20m; // 20% cap
    private const decimal DeliveryFeeFixed = 20.00m;


    public CartService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }



    public async Task<AppResponse<AddToCartResponseDto>> AddItemAsync(
    CartAddItemDto dto,
    CartItemSourceType sourceType,
    CancellationToken ct)
    {
        // 1) Validate request
        var validation = ValidateAddItemRequest(dto);
        if (validation is not null)
            return validation;

        // 2) Load product
        var product = await unitOfWork.Products.GetByIdNoTrackingAsync(dto.ProductId, ct);
        if (product is null)
            return AppResponse<AddToCartResponseDto>.NotFound("Product not found");

        // 3) Validate unit level rules
        var unitValidation = ValidateUnitLevelRules(dto, product);
        if (unitValidation is not null)
            return unitValidation;

        // 4) Validate source-specific rules
        var sourceValidation = await ValidateSourceRulesAsync(product, sourceType, ct);
        if (sourceValidation is not null)
            return sourceValidation;

        // 5) Compute pricing and availability
        var pricing = BuildCartPricing(dto, product);

        // 6) Get or create active cart
        var cart = await GetOrCreateActiveCartAsync(dto, ct);

        // 7) Prevent mixing normal and redemption items
        var mixingValidation = await ValidateCartMixingAsync(cart.Id, sourceType, ct);
        if (mixingValidation is not null)
            return mixingValidation;

        // 8) Upsert cart item
        var upsert = await UpsertCartItemAsync(cart, dto, product, sourceType, pricing.CurrentUnitPrice, ct);

        // 9) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 10) Build response
        var response = BuildAddItemResponse(
            cart,
            upsert.CartItem,
            sourceType,
            pricing.AvailableQty,
            upsert.RequiresRefresh,
            upsert.RefreshReason,
            upsert.PriceChange);

        return AppResponse<AddToCartResponseDto>.Ok(response, "Item added to cart successfully");

        // Future improvements:
        // - Add transaction or retry policy for active cart race conditions
        // - Replace CustomerId with token claims
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

    #region CartPreview

    public async Task<AppResponse<CartPreviewResponseDto>> PreviewAsync(CartPreviewRequestDto dto, CancellationToken ct)
    {
        // 1) Validate request
        var validation = ValidatePreviewRequest(dto);
        if (validation is not null)
            return validation;

        // 2) Validate address ownership
        var addressOk = await unitOfWork.Carts.AddressExistsAsync(dto.AddressId, dto.CustomerId, ct);
        if (!addressOk)
            return AppResponse<CartPreviewResponseDto>.ValidationError("Invalid address");

        // 3) Load active cart for preview
        var cartData = await unitOfWork.Carts.GetActiveCartDataForPreviewAsync(dto.CartId, dto.CustomerId, ct);
        if (cartData is null)
            return AppResponse<CartPreviewResponseDto>.NotFound("Active cart not found");

        // 4) Reject if cart is empty
        if (cartData.Items.Count == 0)
            return FailCannotProceed("Cart is empty");

        // 5) Remove invalid items (hard delete) then reload if needed
        var invalidItemIds = GetInvalidCartItemIds(cartData.Items);
        if (invalidItemIds.Count > 0)
        {
            await unitOfWork.Carts.RemoveCartItemsByIdsAsync(cartData.CartId, invalidItemIds, ct);
            await unitOfWork.CompleteAsync(ct);

            // Reload cart after deletion to keep response accurate
            cartData = await unitOfWork.Carts.GetActiveCartDataForPreviewAsync(dto.CartId, dto.CustomerId, ct);
            if (cartData is null || cartData.Items.Count == 0)
                return FailCannotProceed("Cart has no valid items");
        }

        // 6) Build pricing lines using current product prices
        var pricedItems = BuildPricedItems(cartData.Items);

        // 7) Compute subtotal (current prices only)
        var subtotal = pricedItems.Sum(x => x.LineTotal);

        // 8) Compute promotions discount (group promotions priority; simple MVP)
        var promoDiscountTotal = ComputePromotionDiscountTotal(pricedItems);

        // 9) Compute subtotal after promotions
        var subtotalAfterPromos = Round2(subtotal - promoDiscountTotal);
        if (subtotalAfterPromos < 0) subtotalAfterPromos = 0;

        // 10) Compute max redeemable points (auto max)
        var pointsResult = ComputeMaxRedeemPoints(pricedItems, subtotalAfterPromos, promoDiscountTotal);

        // 11) Compute totals (fixed delivery fee)
        var redeemMoney = pointsResult.RedeemValueMoney;
        var deliveryFee = DeliveryFeeFixed;
        var grandTotal = Round2(subtotalAfterPromos - redeemMoney + deliveryFee);
        if (grandTotal < 0) grandTotal = 0;

        // 12) Build response
        var response = new CartPreviewResponseDto
        {
            CartId = cartData.CartId,
            CanProceed = true,

            Subtotal = Round2(subtotal),
            PromotionDiscountTotal = Round2(promoDiscountTotal),
            SubtotalAfterPromotions = subtotalAfterPromos,

            MaxRedeemablePoints = pointsResult.MaxRedeemablePoints,
            RequestedRedeemPoints = pointsResult.RequestedRedeemPoints,
            AppliedRedeemPoints = pointsResult.AppliedRedeemPoints,
            RedeemValueMoney = pointsResult.RedeemValueMoney,

            DeliveryFee = Round2(deliveryFee),
            GrandTotal = grandTotal,

            HasPriceChanges = pricedItems.Any(x => x.CurrentUnitPrice != x.CurrentUnitPriceSnapshot),
            Warnings = BuildWarnings(pricedItems),

            Items = pricedItems.Select(ToPreviewItemDto).ToList()
        };

        return AppResponse<CartPreviewResponseDto>.Ok(response, "Cart preview calculated successfully");

        // Future improvements:
        // - Replace delete+reload with transactional batch operations
        // - Implement full group promotion engine (buy X get Y)
        // - Move settings to appsettings.json and then database settings
    }

    // -------- Helpers (clean code) --------

    private static AppResponse<CartPreviewResponseDto>? ValidatePreviewRequest(CartPreviewRequestDto dto)
    {
        // 1) Validate identifiers
        if (dto is null)
            return AppResponse<CartPreviewResponseDto>.ValidationError("Request body is required");

        if (dto.CartId <= 0)
            return AppResponse<CartPreviewResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["CartId"] = new[] { "CartId is required" } },
                detail: "Validation failed"
            );

        if (dto.CustomerId <= 0)
            return AppResponse<CartPreviewResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["CustomerId"] = new[] { "CustomerId is required" } },
                detail: "Validation failed"
            );

        if (dto.AddressId <= 0)
            return AppResponse<CartPreviewResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["AddressId"] = new[] { "AddressId is required" } },
                detail: "Validation failed"
            );

        return null;
    }

    private static AppResponse<CartPreviewResponseDto> FailCannotProceed(string message)
    {
        // 1) Return 400 with canProceed=false semantics
        return AppResponse<CartPreviewResponseDto>.Fail(
            error: message,
            errorCode: AppErrorCode.BusinessRuleViolation,
            statusCode: 400,
            title: "Cannot Proceed",
            detail: message
        );
    }

    private static List<int> GetInvalidCartItemIds(List<CartPreviewItemData> items)
    {
        // 1) Detect invalid items based on product state and unit rules
        var invalid = new List<int>();

        foreach (var i in items)
        {
            var productInvalid = i.DeletedAt != null || !i.IsActive || !i.IsAvailable;
            if (productInvalid)
            {
                invalid.Add(i.CartItemId);
                continue;
            }

            // Inner unit requires split sale and inner pricing data
            if (i.UnitLevel == 2)
            {
                var innerInvalid = !i.AllowSplitSale || i.InnerUnitPrice is null || i.InnerPerOuter is null || i.InnerPerOuter < 1;
                if (innerInvalid)
                    invalid.Add(i.CartItemId);
            }
        }

        return invalid;
    }

    private static List<PricedCartItem> BuildPricedItems(List<CartPreviewItemData> items)
    {
        // 1) Map items and compute current price, available qty, and line totals
        var result = new List<PricedCartItem>();

        foreach (var i in items)
        {
            var currentPrice = (i.UnitLevel == 1) ? i.OuterUnitPrice : (i.InnerUnitPrice ?? 0m);

            var availableQty = (i.UnitLevel == 1)
                ? i.StockOuterQty
                : (i.InnerPerOuter.HasValue ? i.StockOuterQty * i.InnerPerOuter.Value : 0m);

            var exceeds = i.Quantity > availableQty;

            var lineTotal = i.Quantity * currentPrice;

            result.Add(new PricedCartItem
            {
                CartItemId = i.CartItemId,
                ProductId = i.ProductId,
                UnitLevel = i.UnitLevel,
                Quantity = i.Quantity,

                NameAr = i.NameAr,
                NameEn = i.NameEn,
                PrimaryImageUrl = i.PrimaryImageUrl,

                CurrentUnitPrice = Round2(currentPrice),
                CurrentUnitPriceSnapshot = Round2(i.CurrentUnitPriceSnapshot),

                AvailableQty = availableQty,
                ExceedsAvailableQty = exceeds,

                LineTotal = Round2(lineTotal),

                // Promotion hooks
                HasProductPromotion = IsProductPromotionActive(i),
                ProductPromotionPercent = i.PromotionDiscountPercent,
                IsInGroupPromotion = i.IsInGroupPromotion,

                Points = i.Points
            });
        }

        return result;
    }

    private static bool IsProductPromotionActive(CartPreviewItemData i)
    {
        // 1) Check product-level promotion window
        if (!i.HasPromotion) return false;
        if (i.PromotionStartsAt is null || i.PromotionEndsAt is null) return false;

        var now = DateTime.UtcNow;
        return i.PromotionStartsAt <= now && i.PromotionEndsAt >= now;
    }

    private static decimal ComputePromotionDiscountTotal(List<PricedCartItem> items)
    {
        // 1) Apply group promotions first (MVP placeholder)
        var groupDiscount = 0m;

        // TODO: Implement group promotions engine (buy X get Y) later

        // 2) Apply product promotions only when item is NOT in group promotion
        var productDiscount = 0m;

        foreach (var i in items)
        {
            if (i.IsInGroupPromotion) continue; // Group promotions override product promos
            if (!i.HasProductPromotion) continue;

            var percent = i.ProductPromotionPercent;
            if (percent <= 0) continue;

            productDiscount += i.LineTotal * (percent / 100m);
        }

        return Round2(groupDiscount + productDiscount);
    }

    private static PointsCalcResult ComputeMaxRedeemPoints(List<PricedCartItem> items, decimal subtotalAfterPromos, decimal promoDiscountTotal)
    {
        // 1) Determine eligibility for points (simple MVP)
        // Note: if you want "no points when any promo exists", enforce here later by a flag
        var pointsAllowed = true;

        // TODO: If promo exists and promo does not allow points, set pointsAllowed=false

        if (!pointsAllowed)
        {
            return new PointsCalcResult
            {
                MaxRedeemablePoints = 0,
                RequestedRedeemPoints = 0,
                AppliedRedeemPoints = 0,
                RedeemValueMoney = 0
            };
        }

        // 2) Eligible subtotal for points (exclude products with Points=0)
        var eligibleSubtotal = items
            .Where(x => x.Points > 0)
            .Sum(x => x.LineTotal);

        // Points discount is applied after promotions
        var baseForCap = Math.Min(subtotalAfterPromos, Round2(eligibleSubtotal));

        // 3) Apply cap (20% of subtotal after promotions)
        var capMoney = Round2(baseForCap * RedeemCapPercent);

        // 4) Convert money cap to points
        var capPoints = (int)Math.Floor(capMoney * ConversionRate);

        // 5) Enforce minimum redemption
        if (capPoints < MinimumRedeemPoints)
        {
            return new PointsCalcResult
            {
                MaxRedeemablePoints = capPoints,
                RequestedRedeemPoints = 0,
                AppliedRedeemPoints = 0,
                RedeemValueMoney = 0
            };
        }

        // 6) Auto-use max allowed points
        var appliedPoints = capPoints;
        var redeemMoney = Round2(appliedPoints / (decimal)ConversionRate);

        return new PointsCalcResult
        {
            MaxRedeemablePoints = capPoints,
            RequestedRedeemPoints = capPoints,
            AppliedRedeemPoints = appliedPoints,
            RedeemValueMoney = redeemMoney
        };
    }

    private static List<string> BuildWarnings(List<PricedCartItem> items)
    {
        // 1) Build warning messages (simple)
        var warnings = new List<string>();

        if (items.Any(x => x.CurrentUnitPrice != x.CurrentUnitPriceSnapshot))
            warnings.Add("Prices have changed since items were added to the cart.");

        if (items.Any(x => x.ExceedsAvailableQty))
            warnings.Add("Some items exceed available quantity.");

        return warnings;
    }

    private static CartPreviewItemDto ToPreviewItemDto(PricedCartItem x)
    {
        // 1) Map priced item to response DTO
        return new CartPreviewItemDto
        {
            CartItemId = x.CartItemId,
            ProductId = x.ProductId,
            UnitLevel = x.UnitLevel,
            Quantity = x.Quantity,

            NameAr = x.NameAr,
            NameEn = x.NameEn,
            PrimaryImageUrl = x.PrimaryImageUrl,

            CurrentUnitPrice = x.CurrentUnitPrice,
            CurrentUnitPriceSnapshot = x.CurrentUnitPriceSnapshot,
            LineTotal = x.LineTotal,

            AvailableQty = x.AvailableQty,
            ExceedsAvailableQty = x.ExceedsAvailableQty,

            IsInGroupPromotion = x.IsInGroupPromotion
        };
    }

    private static decimal Round2(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    #endregion



    // -------------------------
    // Helpers (private methods)
    // -------------------------
    private static AppResponse<AddToCartResponseDto>? ValidateAddItemRequest(CartAddItemDto dto)
    {
        // 1) Validate basic input
        if (dto is null)
            return AppResponse<AddToCartResponseDto>.ValidationError("Request body is required");

        if (dto.CustomerId <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["CustomerId"] = new[] { "CustomerId is required" } },
                detail: "Validation failed");

        if (dto.StoreId <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["StoreId"] = new[] { "StoreId is required" } },
                detail: "Validation failed");

        if (dto.ProductId <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["ProductId"] = new[] { "ProductId is required" } },
                detail: "Validation failed");

        if (dto.Quantity <= 0)
            return AppResponse<AddToCartResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["Quantity"] = new[] { "Quantity must be greater than 0" } },
                detail: "Validation failed");

        return null;
    }

    private static AppResponse<AddToCartResponseDto>? ValidateUnitLevelRules(CartAddItemDto dto, Product product)
    {
        // 1) Validate inner unit rules
        if (dto.UnitLevel == UnitLevel.Inner)
        {
            if (!product.AllowSplitSale ||
                product.InnerUnitPrice is null ||
                product.InnerPerOuter is null ||
                product.InnerPerOuter < 1)
            {
                return AppResponse<AddToCartResponseDto>.BusinessRuleViolation(
                    "Inner unit sale is not allowed for this product");
            }
        }

        return null;
    }

    private async Task<AppResponse<AddToCartResponseDto>?> ValidateSourceRulesAsync(Product product,CartItemSourceType sourceType,CancellationToken ct)
    {
        // 1) Normal items do not need redemption checks
        if (sourceType == CartItemSourceType.Normal)
            return null;

        // 2) Product must be redeemable by points
        if (!product.IsRedeemableByPoints)
            return AppResponse<AddToCartResponseDto>.BusinessRuleViolation(
                "Product is not redeemable by points");

        // 3) Product must not have an active direct promotion
        if (HasActiveProductPromotion(product))
            return AppResponse<AddToCartResponseDto>.BusinessRuleViolation(
                "Redeemable product cannot have an active promotion");

        // 4) Product must not be in an active group promotion
        var inGroupPromotion = await unitOfWork.Carts.IsProductInActiveGroupPromotionAsync(product.Id, ct);
        if (inGroupPromotion)
            return AppResponse<AddToCartResponseDto>.BusinessRuleViolation(
                "Redeemable product cannot be in an active group promotion");

        return null;
    }

    private static bool HasActiveProductPromotion(Product product)
    {
        // 1) Check product-level promotion window
        if (!product.HasPromotion)
            return false;

        if (product.PromotionStartsAt is null || product.PromotionEndsAt is null)
            return false;

        var now = DateTime.UtcNow;

        return product.PromotionStartsAt <= now && product.PromotionEndsAt >= now;
    }

    private static CartPricingInfo BuildCartPricing(CartAddItemDto dto, Product product)
    {
        // 1) Compute current unit price
        var currentUnitPrice = dto.UnitLevel == UnitLevel.Outer
            ? product.OuterUnitPrice
            : product.InnerUnitPrice!.Value;

        // 2) Compute available quantity in selected unit
        var availableQty = dto.UnitLevel == UnitLevel.Outer
            ? product.Quantity
            : product.Quantity * product.InnerPerOuter!.Value;

        return new CartPricingInfo
        {
            CurrentUnitPrice = currentUnitPrice,
            AvailableQty = availableQty
        };
    }

    private async Task<Cart> GetOrCreateActiveCartAsync(CartAddItemDto dto, CancellationToken ct)
    {
        // 1) Load active cart
        var cart = await unitOfWork.Carts.GetActiveByCustomerAsync(dto.CustomerId, ct);
        if (cart is not null)
            return cart;

        // 2) Create active cart
        var now = DateTime.UtcNow;

        cart = new Cart
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

        await unitOfWork.Carts.AddCartAsync(cart, ct);
        await unitOfWork.CompleteAsync(ct);

        return cart;

        // Future improvement:
        // - Handle unique active cart race condition with retry
    }

    private async Task<AppResponse<AddToCartResponseDto>?> ValidateCartMixingAsync(int cartId,CartItemSourceType sourceType,CancellationToken ct)
    {
        // 1) Prevent mixing normal and points redemption items
        var hasDifferent = await unitOfWork.Carts.HasDifferentSourceTypeAsync(cartId, sourceType, ct);

        if (hasDifferent)
        {
            return AppResponse<AddToCartResponseDto>.BusinessRuleViolation(
                "Cart cannot contain normal items and points redemption items together");
        }

        return null;
    }


    private async Task<CartItemUpsertResult> UpsertCartItemAsync(
        Cart cart,
        CartAddItemDto dto,
        Product product,
        CartItemSourceType sourceType,
        decimal currentUnitPrice,
        CancellationToken ct)
    {
        // 1) Load existing item by product, unit, and source
        var cartItem = await unitOfWork.Carts.GetByCartProductUnitSourceAsync(
            cart.Id,
            dto.ProductId,
            dto.UnitLevel,
            sourceType,
            ct);

        var now = DateTime.UtcNow;

        var result = new CartItemUpsertResult();

        if (cartItem is null)
        {
            // 2) Create new cart item
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                UnitLevel = (byte)dto.UnitLevel,
                SourceType = (byte)sourceType,

                Quantity = dto.Quantity,

                UnitPriceSnapshot = currentUnitPrice,
                CurrentUnitPriceSnapshot = currentUnitPrice,

                MinOrderQtySnapshot = product.MinOrderQty,
                MaxOrderQtySnapshot = product.MaxOrderQty,
                MaxPerDayQtySnapshot = product.MaxPerDayQty,

                IsValid = true,
                InvalidReason = null,

                CreatedAt = now
            };

            // 3) Mark item invalid if product state requires it
            MarkInvalidIfNeeded(cart, cartItem, product, now);

            await unitOfWork.Carts.AddCartItemAsync(cartItem, ct);

            result.CartItem = cartItem;
            return result;
        }

        // 4) Increase existing quantity
        cartItem.Quantity += dto.Quantity;
        cartItem.UpdatedAt = now;

        // 5) Detect price changes
        if (cartItem.CurrentUnitPriceSnapshot != currentUnitPrice)
        {
            cartItem.UnitPriceSnapshot = cartItem.CurrentUnitPriceSnapshot;
            cartItem.CurrentUnitPriceSnapshot = currentUnitPrice;

            result.RequiresRefresh = true;
            result.RefreshReason = "Price changed. Refresh cart before checkout.";
            result.PriceChange = new ItemPriceChangeDto
            {
                OldPrice = cartItem.UnitPriceSnapshot,
                NewPrice = cartItem.CurrentUnitPriceSnapshot,
                ChangedAtUtc = now
            };

            SetCartStatus(cart, CartStatus.Invalid, result.RefreshReason, now);
        }

        // 6) Update snapshots
        cartItem.MinOrderQtySnapshot = product.MinOrderQty;
        cartItem.MaxOrderQtySnapshot = product.MaxOrderQty;
        cartItem.MaxPerDayQtySnapshot = product.MaxPerDayQty;

        // 7) Re-check item validity
        MarkInvalidIfNeeded(cart, cartItem, product, now);

        unitOfWork.Carts.UpdateCartItem(cartItem);

        result.CartItem = cartItem;
        return result;
    }

    private static void MarkInvalidIfNeeded(Cart cart, CartItem item, Product product, DateTime nowUtc)
    {
        // 1) Check product invalid states
        var isInvalid =
            product.DeletedAt != null ||
            product.IsActive == false ||
            product.IsAvailable == false;

        if (!isInvalid)
        {
            item.IsValid = true;
            item.InvalidReason = null;
            return;
        }

        item.IsValid = false;
        item.InvalidReason = product.DeletedAt != null
            ? "Product was deleted"
            : product.IsActive == false
                ? "Product is inactive"
                : "Product is unavailable";

        item.UpdatedAt = nowUtc;

        SetCartStatus(cart, CartStatus.Invalid, "Cart contains invalid items. Remove them to continue.", nowUtc);
    }

    private static void SetCartStatus(Cart cart, CartStatus status, string? reason, DateTime nowUtc)
    {
        // 1) Set cart status
        cart.Status = (byte)status;
        cart.StatusUpdatedAt = nowUtc;
        cart.ExpiredReason = string.IsNullOrWhiteSpace(reason) ? null : reason;
        cart.UpdatedAt = nowUtc;
    }

    private static AddToCartResponseDto BuildAddItemResponse(
        Cart cart,
        CartItem cartItem,
        CartItemSourceType sourceType,
        decimal availableQty,
        bool requiresRefresh,
        string? refreshReason,
        ItemPriceChangeDto? priceChange)
    {
        // 1) Build response DTO
        return new AddToCartResponseDto
        {
            CartId = cart.Id,
            CartStatus = (CartStatus)cart.Status,

            CartItemId = cartItem.Id,
            ProductId = cartItem.ProductId,

            UnitLevel = (UnitLevel)cartItem.UnitLevel,
            SourceType = sourceType,

            Quantity = cartItem.Quantity,

            AvailableQty = availableQty,
            ExceedsAvailableQty = cartItem.Quantity > availableQty,

            RequiresRefresh = requiresRefresh || (CartStatus)cart.Status == CartStatus.Invalid,
            RefreshReason = refreshReason,

            PriceChange = priceChange
        };
    }


}
