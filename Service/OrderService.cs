using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Service.Models.Checkout;
using Shared.Enums.Order;
using Shared.Models.Dtos.Order;
using Shared.Responses;

namespace Service;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork unitOfWork;

    // Simple settings (later move to appsettings.json)
    private const int ConversionRate = 30;          // 30 points = 1 money unit
    private const int MinimumRedeemPoints = 500;    // minimum points
    private const decimal RedeemCapPercent = 0.20m; // 20% cap
    private const decimal DeliveryFeeFixed = 10.00m;

    public OrderService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public async Task<AppResponse<CheckoutResultDto>> CheckoutAsync(CheckoutRequestDto dto, CancellationToken ct)
    {
        // 1) Validate request
        var validation = ValidateCheckoutRequest(dto);
        if (validation is not null) return validation;

        // 2) Load address snapshot
        var (addressOk, addressSnap) = await unitOfWork.Orders.GetAddressSnapshotAsync(dto.AddressId, dto.CustomerId, ct);
        if (!addressOk) return AppResponse<CheckoutResultDto>.ValidationError("Invalid address");

        // 3) Load checkout lines from source
        var lines = await LoadCheckoutLinesAsync(dto, ct);
        if (lines.Count == 0) return AppResponse<CheckoutResultDto>.BusinessRuleViolation("Source has no items");

        // 4) Build priced items using current prices
        var priced = BuildPricedItems(lines);

        // 5) Calculate promotions (group first, then product promos)
        var promo = await CalculatePromotionsAsync(priced, ct);

        // 6) Apply promotions to items and compute totals
        ApplyDiscounts(priced, promo);

        // 7) Calculate points (blocked if any promotion applied)
        var points = CalculatePoints(priced, promo.PromotionDiscountTotal);

        // 8) Calculate final totals
        var totals = CalculateTotals(priced, promo.PromotionDiscountTotal, points.RedeemedAmount);

        // 9) Create order and order items (snapshots)
        var order =  await CreateOrderAsync(dto, lines, priced, totals, points, addressSnap, promo, ct);

        // 10) Update source state (cart checked out / prescription closed)
        await UpdateSourceAfterCheckoutAsync(dto, lines, ct);

        // 11) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 12) Build response
        var response = BuildCheckoutResponse(order, priced, totals, promo, points);

        return AppResponse<CheckoutResultDto>.Created(response, location: $"/api/v1/orders/{order.Id}", title: "Order created successfully");

        // Future improvements:
        // - Add transaction scope for stronger guarantees across multiple updates
        // - Add idempotency key to prevent duplicate orders on network retries
    }

    public async Task<AppResponse<CheckoutPreviewResponseDto>> PreviewAsync(CheckoutPreviewRequestDto dto, CancellationToken ct)
    {
        // 1) Validate request
        var validation = ValidateCheckoutPreviewRequest(dto);
        if (validation is not null) return validation;

        // 2) Load address snapshot (ownership check)
        var (addressOk, _) = await unitOfWork.Orders.GetAddressSnapshotAsync(dto.AddressId, dto.CustomerId, ct);
        if (!addressOk) return AppResponse<CheckoutPreviewResponseDto>.ValidationError("Invalid address");

        // 3) Load checkout lines from source (cart or prescription)
        var lines = await LoadCheckoutLinesAsync(dto.CustomerId, dto.SourceType, dto.SourceId, ct);
        if (lines.Count == 0) return AppResponse<CheckoutPreviewResponseDto>.BusinessRuleViolation("Source has no items");

        // 4) Build priced items using current prices (reuses existing helper)
        var priced = BuildPricedItems(lines);

        // 5) Calculate promotions (group first, then product promos) (reuses existing helper)
        var promo = await CalculatePromotionsAsync(priced, ct);

        // 6) Apply discounts and compute line totals (reuses existing helper)
        ApplyDiscounts(priced, promo);

        // 7) Calculate points (blocked if any promotion applied) (reuses existing helper)
        var points = CalculatePoints(priced, promo.PromotionDiscountTotal);

        // 8) Calculate totals (reuses existing helper)
        var totals = CalculateTotals(priced, promo.PromotionDiscountTotal, points.RedeemedAmount);

        // 9) Build response
        var response = BuildPreviewResponse(dto, priced, promo, points, totals, lines);

        return AppResponse<CheckoutPreviewResponseDto>.Ok(response, "Checkout preview calculated successfully");

        // Future improvements:
        // - Add customer points balance check (Ledger) before applying points
        // - Add stock validation blocking if you decide to enforce it at preview
    }



    // -------------------------
    // Step helpers
    // -------------------------

    private static AppResponse<CheckoutResultDto>? ValidateCheckoutRequest(CheckoutRequestDto dto)
    {
        // 1) Validate basic fields
        if (dto is null) return AppResponse<CheckoutResultDto>.ValidationError("Request body is required");

        if (dto.CustomerId <= 0)
            return AppResponse<CheckoutResultDto>.ValidationErrors(new() { ["CustomerId"] = new[] { "CustomerId is required" } }, "Validation failed");

        if (dto.SourceId <= 0)
            return AppResponse<CheckoutResultDto>.ValidationErrors(new() { ["SourceId"] = new[] { "SourceId is required" } }, "Validation failed");

        if (dto.AddressId <= 0)
            return AppResponse<CheckoutResultDto>.ValidationErrors(new() { ["AddressId"] = new[] { "AddressId is required" } }, "Validation failed");

        return null;
    }

    private async Task<List<CheckoutLineData>> LoadCheckoutLinesAsync(CheckoutRequestDto dto, CancellationToken ct)
    {
        // 1) Load lines from cart or prescription
        return dto.SourceType switch
        {
            CheckoutSourceType.Cart => await unitOfWork.Orders.GetLinesFromCartAsync(dto.SourceId, dto.CustomerId, ct),
            CheckoutSourceType.Prescription => await unitOfWork.Orders.GetLinesFromPrescriptionAsync(dto.SourceId, dto.CustomerId, ct),
            _ => new List<CheckoutLineData>()
        };

        // Future improvement: validate source ownership using token claims
    }

    private static List<PricedCheckoutItem> BuildPricedItems(List<CheckoutLineData> lines)
    {
        // 1) Build current pricing per line
        var items = new List<PricedCheckoutItem>();

        foreach (var l in lines)
        {
            var unitPrice = l.UnitLevel == 1 ? l.OuterUnitPrice : (l.InnerUnitPrice ?? 0m);

            items.Add(new PricedCheckoutItem
            {
                ProductId = l.ProductId,
                UnitLevel = l.UnitLevel,
                Quantity = l.Quantity,

                OuterUnitIdSnapshot = l.OuterUnitId,
                InnerUnitIdSnapshot = l.InnerUnitId,
                InnerPerOuterSnapshot = l.InnerPerOuter,

                UnitPrice = Round2(unitPrice),
                DiscountPercent = 0m,
                AppliedPromotionId = null,

                PointsPerUnit = l.Points
            });
        }

        // 2) Compute line subtotals
        foreach (var i in items)
        {
            i.LineSubtotal = Round2(i.Quantity * i.UnitPrice);
            i.FinalUnitPrice = i.UnitPrice;
            i.LineTotal = i.LineSubtotal;
        }

        return items;
    }

    private async Task<PromotionCalcResult> CalculatePromotionsAsync(List<PricedCheckoutItem> items, CancellationToken ct)
    {
        // 1) Load group promotions membership by product
        var productIds = items.Select(x => x.ProductId).Distinct().ToList();
        var groupByProduct = await unitOfWork.Orders.GetActiveGroupPromotionsByProductAsync(productIds, ct);

        // 2) Load product-level promo percent
        var productPromoPerc = await unitOfWork.Orders.GetActiveProductPromotionPercentAsync(productIds, ct);

        // 3) Apply group promo priority (mark items in group)
        foreach (var i in items)
        {
            if (groupByProduct.TryGetValue(i.ProductId, out var gp))
            {
                i.AppliedPromotionId = gp.PromotionId;
                i.GroupPromotion = gp;
            }
        }

        // 4) Calculate group promotion discount (cheapest items discounted)
        var groupDiscount = CalculateGroupPromotionDiscount(items);

        // 5) Calculate product promotion discount only when not in group promo
        var productDiscount = 0m;
        foreach (var i in items)
        {
            if (i.GroupPromotion != null) continue;

            if (productPromoPerc.TryGetValue(i.ProductId, out var percent) && percent > 0)
            {
                productDiscount += i.LineSubtotal * (percent / 100m);
                i.DiscountPercent = percent;
            }
        }

        var total = Round2(groupDiscount + productDiscount);

        return new PromotionCalcResult
        {
            PromotionDiscountTotal = total,
            AnyPromotionApplied = total > 0m
        };

        // Future improvements:
        // - Choose best group promotion across cart (not best per product)
        // - Support multi-rule promotions (buy X from list, discount Y from another list)
    }

    private static decimal CalculateGroupPromotionDiscount(List<PricedCheckoutItem> items)
    {
        // 1) Group items by applied group promotion id
        var groups = items.Where(x => x.GroupPromotion != null)
                          .GroupBy(x => x.GroupPromotion!.PromotionId);

        var totalDiscount = 0m;

        foreach (var g in groups)
        {
            var promo = g.First().GroupPromotion!;
            var basic = promo.BasicAmount;
            var offer = promo.OfferAmount;

            if (basic <= 0 || offer <= 0) continue;

            var bundleSize = basic + offer;
            var totalQty = g.Sum(x => x.Quantity);

            // 2) Compute how many discounted units apply
            var times = (int)Math.Floor(totalQty / bundleSize);
            var discountedUnits = times * offer;

            if (discountedUnits <= 0) continue;

            // 3) Build unit-price list to discount cheapest units first
            var unitPrices = ExpandUnitPrices(g.ToList());

            // 4) Apply discount on cheapest units
            foreach (var price in unitPrices.Take(discountedUnits))
            {
                totalDiscount += price * (promo.DiscountPercent / 100m);
            }

            // 5) Mark discount percent for UI snapshots (optional, keep 0 for now)
            foreach (var i in g)
            {
                i.DiscountPercent = Math.Max(i.DiscountPercent, promo.DiscountPercent);
            }
        }

        return Round2(totalDiscount);

        // Future improvements:
        // - Handle fractional quantities if you allow decimals (currently quantities can be decimal)
        // - Decide whether inner unit counts as 1 unit or needs conversion rules
    }

    private static List<decimal> ExpandUnitPrices(List<PricedCheckoutItem> items)
    {
        // 1) Expand unit prices for cheapest-first selection (MVP assumes integer-like qty)
        var list = new List<decimal>();

        foreach (var i in items)
        {
            var count = (int)Math.Floor(i.Quantity); // MVP simplification
            for (var k = 0; k < count; k++)
                list.Add(i.UnitPrice);
        }

        // 2) Sort ascending for cheapest-first
        list.Sort();
        return list;
    }

    private static void ApplyDiscounts(List<PricedCheckoutItem> items, PromotionCalcResult promo)
    {
        // 1) Apply discount percent (product promo or group promo marker)
        foreach (var i in items)
        {
            var discount = i.DiscountPercent / 100m;
            i.FinalUnitPrice = Round2(i.UnitPrice * (1m - discount));
            if (i.FinalUnitPrice < 0) i.FinalUnitPrice = 0;

            i.LineTotal = Round2(i.Quantity * i.FinalUnitPrice);
        }

        // Future improvements:
        // - Apply exact group discount distribution per line rather than percent marker
    }

    private static PointsCalcResult CalculatePoints(List<PricedCheckoutItem> items, decimal promotionDiscountTotal)
    {
        // 1) Block points if any promotion is applied
        if (promotionDiscountTotal > 0m)
        {
            return new PointsCalcResult { RedeemedPoints = 0, RedeemedAmount = 0m, MaxRedeemablePoints = 0 };
        }

        // 2) Eligible subtotal (items with PointsPerUnit > 0)
        var eligibleSubtotal = items.Where(x => x.PointsPerUnit > 0).Sum(x => x.LineTotal);

        // 3) Apply cap and minimum
        var capMoney = Round2(eligibleSubtotal * RedeemCapPercent);
        var capPoints = (int)Math.Floor(capMoney * ConversionRate);

        if (capPoints < MinimumRedeemPoints)
            return new PointsCalcResult { RedeemedPoints = 0, RedeemedAmount = 0m, MaxRedeemablePoints = capPoints };

        // 4) Auto-use max allowed points (balance integration later)
        var redeemedPoints = capPoints;
        var redeemedAmount = Round2(redeemedPoints / (decimal)ConversionRate);

        return new PointsCalcResult
        {
            MaxRedeemablePoints = capPoints,
            RedeemedPoints = redeemedPoints,
            RedeemedAmount = redeemedAmount
        };

        // Future improvements:
        // - Limit by customer points balance (Ledger/Balance table)
        // - Support allowing points with some promotions by policy flag
    }

    private static TotalsCalcResult CalculateTotals(List<PricedCheckoutItem> items, decimal promoDiscountTotal, decimal redeemedAmount)
    {
        // 1) Subtotal from current prices
        var subtotal = Round2(items.Sum(x => x.LineSubtotal));

        // 2) Subtotal after promotions
        var afterPromos = Round2(subtotal - promoDiscountTotal);
        if (afterPromos < 0) afterPromos = 0;

        // 3) Delivery and grand total
        var delivery = Round2(DeliveryFeeFixed);
        var grand = Round2(afterPromos - redeemedAmount + delivery);
        if (grand < 0) grand = 0;

        return new TotalsCalcResult
        {
            Subtotal = subtotal,
            PromotionDiscountTotal = Round2(promoDiscountTotal),
            SubtotalAfterPromotions = afterPromos,
            DeliveryFee = delivery,
            GrandTotal = grand
        };
    }
    
    private async Task<Order> CreateOrderAsync(
        CheckoutRequestDto dto,
        List<CheckoutLineData> sourceLines,
        List<PricedCheckoutItem> priced,
        TotalsCalcResult totals,
        PointsCalcResult points,
        CustomerAddressSnapshot address,
        PromotionCalcResult promo,
        CancellationToken ct)
    {
        // 1) Build order entity
        var now = DateTime.UtcNow;

        // 2) Resolve store/prescription from source
        var storeId = sourceLines.First().StoreId;
        var prescriptionId = sourceLines.First().PrescriptionId;

        // 3) Create order object (tracked)
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = dto.CustomerId,
            StoreId = storeId,
            PrescriptionId = prescriptionId,

            Status = 1, // 1=Pending
            StatusUpdatedAt = now,

            PaymentMethod = (byte)dto.PaymentMethod,
            PaymentStatus = 1, // 1=Unpaid

            Subtotal = totals.Subtotal,
            ItemsDiscountTotal = totals.PromotionDiscountTotal,
            DeliveryFee = totals.DeliveryFee,
            RedeemedPoints = points.RedeemedPoints,
            RedeemedAmount = points.RedeemedAmount,
            GrandTotal = totals.GrandTotal,
            EarnedPoints = 0, // Will be calculated later

            AddressId = address.AddressId,
            DeliveryTitle = address.Title,
            DeliveryCity = address.City,
            DeliveryRegion = address.Region,
            DeliveryStreet = address.Street,
            DeliveryLatitude = address.Latitude,
            DeliveryLongitude = address.Longitude,
            DeliveryPhone = address.Phone,

            CreatedAt = now
        };

        // 4) Add order using OrderRepository
        await unitOfWork.Orders.AddOrderAsync(order, ct);

        // 5) Save now to get OrderId
        await unitOfWork.CompleteAsync(ct);

        // 6) Build order items (snapshots)
        var orderItems = priced.Select(i => new OrderItem
        {
            OrderId = order.Id,
            ProductId = i.ProductId,

            UnitLevel = i.UnitLevel,
            Quantity = i.Quantity,

            OuterUnitIdSnapshot = i.OuterUnitIdSnapshot,
            InnerUnitIdSnapshot = i.InnerUnitIdSnapshot,
            InnerPerOuterSnapshot = i.InnerPerOuterSnapshot,

            UnitPriceSnapshot = i.UnitPrice,
            DiscountPercentSnapshot = i.DiscountPercent,
            FinalUnitPriceSnapshot = i.FinalUnitPrice,
            LineTotal = i.LineTotal,

            PointsSnapshot = i.PointsPerUnit,
            EarnedPoints = 0,

            AppliedPromotionId = i.GroupPromotion?.PromotionId,

            CreatedAt = now
        }).ToList();

        // 7) Add order items using OrderRepository
        await unitOfWork.Orders.AddItemsRangeAsync(orderItems, ct);

        // 8) Return order (save will happen outside or in caller after adding other changes)
        return order;

        // Future improvements:
        // - Wrap order + items in a single transaction scope (if not already)
        // - Generate sequential order numbers per store
        // - Compute EarnedPoints based on your ERP logic
    }

    private async Task UpdateSourceAfterCheckoutAsync(CheckoutRequestDto dto, List<CheckoutLineData> lines, CancellationToken ct)
    {
        // 1) Update source state after order creation
        if (dto.SourceType == CheckoutSourceType.Cart && lines.First().CartId.HasValue)
            await unitOfWork.Orders.MarkCartCheckedOutAsync(lines.First().CartId!.Value, ct);

        if (dto.SourceType == CheckoutSourceType.Prescription && lines.First().PrescriptionId.HasValue)
            await unitOfWork.Orders.ClosePrescriptionAsync(lines.First().PrescriptionId!.Value, ct);

        // Future improvements:
        // - Handle failures with transaction and retries
    }

    private static CheckoutResultDto BuildCheckoutResponse(Order order, List<PricedCheckoutItem> items, TotalsCalcResult totals, PromotionCalcResult promo, PointsCalcResult points)
    {
        // 1) Map result
        return new CheckoutResultDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,

            Subtotal = totals.Subtotal,
            PromotionDiscountTotal = totals.PromotionDiscountTotal,
            SubtotalAfterPromotions = totals.SubtotalAfterPromotions,

            RedeemedPoints = points.RedeemedPoints,
            RedeemedAmount = points.RedeemedAmount,

            DeliveryFee = totals.DeliveryFee,
            GrandTotal = totals.GrandTotal,

            Items = items.Select(i => new CheckoutItemResultDto
            {
                ProductId = i.ProductId,
                UnitLevel = i.UnitLevel,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                FinalUnitPrice = i.FinalUnitPrice,
                LineTotal = i.LineTotal,
                AppliedPromotionId = i.GroupPromotion?.PromotionId
            }).ToList()
        };
    }

    private static string GenerateOrderNumber()
    {
        // 1) Generate simple order number (MVP)
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        // Future improvement: use database sequence per store
    }

    private static decimal Round2(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);


    // --- preivew
    private static AppResponse<CheckoutPreviewResponseDto>? ValidateCheckoutPreviewRequest(CheckoutPreviewRequestDto dto)
    {
        // 1) Validate basic fields
        if (dto is null) return AppResponse<CheckoutPreviewResponseDto>.ValidationError("Request body is required");

        if (dto.CustomerId <= 0)
            return AppResponse<CheckoutPreviewResponseDto>.ValidationErrors(new() { ["CustomerId"] = new[] { "CustomerId is required" } }, "Validation failed");

        if (dto.SourceId <= 0)
            return AppResponse<CheckoutPreviewResponseDto>.ValidationErrors(new() { ["SourceId"] = new[] { "SourceId is required" } }, "Validation failed");

        if (dto.AddressId <= 0)
            return AppResponse<CheckoutPreviewResponseDto>.ValidationErrors(new() { ["AddressId"] = new[] { "AddressId is required" } }, "Validation failed");

        return null;
    }

    private async Task<List<CheckoutLineData>> LoadCheckoutLinesAsync(
        int customerId, CheckoutSourceType sourceType, int sourceId, CancellationToken ct)
    {
        // 1) Load lines from repository based on source type
        return sourceType switch
        {
            CheckoutSourceType.Cart => await unitOfWork.Orders.GetLinesFromCartAsync(sourceId, customerId, ct),
            CheckoutSourceType.Prescription => await unitOfWork.Orders.GetLinesFromPrescriptionAsync(sourceId, customerId, ct),
            _ => new List<CheckoutLineData>()
        };
    }

    private static CheckoutPreviewResponseDto BuildPreviewResponse(
        CheckoutPreviewRequestDto dto,
        List<PricedCheckoutItem> priced,
        PromotionCalcResult promo,
        PointsCalcResult points,
        TotalsCalcResult totals,
        List<CheckoutLineData> sourceLines)
    {
        // 1) Build warnings (simple)
        var warnings = new List<string>();
        var hasPriceChanges = false;

        // Price change comparison is only available if source is cart and snapshot exists in lines
        if (dto.SourceType == CheckoutSourceType.Cart)
        {
            // 2) Detect price changes using cart snapshot (warning only)
            var snapshotByProductUnit = sourceLines
                .Where(x => x.CartId.HasValue)
                .GroupBy(x => (x.ProductId, x.UnitLevel))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var i in priced)
            {
                if (snapshotByProductUnit.TryGetValue((i.ProductId, i.UnitLevel), out var line))
                {
                    // Note: Cart snapshot is in CartItems.CurrentUnitPriceSnapshot; if not present in CheckoutLineData, ignore.
                    // If you want it, add CurrentUnitPriceSnapshot to CheckoutLineData in repository query.
                }
            }
        }

        // 3) Optional warnings
        if (promo.PromotionDiscountTotal > 0)
            warnings.Add("A promotion discount is applied. Points redemption is disabled.");

        // 4) Map items
        var items = priced.Select(i => new CheckoutPreviewItemDto
        {
            ProductId = i.ProductId,
            UnitLevel = i.UnitLevel,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountPercent = i.DiscountPercent,
            FinalUnitPrice = i.FinalUnitPrice,
            LineTotal = i.LineTotal,
            AppliedPromotionId = i.GroupPromotion?.PromotionId,
            SnapshotPrice = null // Fill later if you add snapshot to CheckoutLineData
        }).ToList();

        // 5) Build response object
        return new CheckoutPreviewResponseDto
        {
            SourceType = dto.SourceType,
            SourceId = dto.SourceId,

            Subtotal = totals.Subtotal,
            PromotionDiscountTotal = totals.PromotionDiscountTotal,
            SubtotalAfterPromotions = totals.SubtotalAfterPromotions,

            MaxRedeemablePoints = points.MaxRedeemablePoints,
            RequestedRedeemPoints = points.MaxRedeemablePoints, // Auto max
            AppliedRedeemPoints = points.RedeemedPoints,
            RedeemValueMoney = points.RedeemedAmount,

            DeliveryFee = totals.DeliveryFee,
            GrandTotal = totals.GrandTotal,

            HasPriceChanges = hasPriceChanges,
            Warnings = warnings,

            Items = items
        };
    }

}
