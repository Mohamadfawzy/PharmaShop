using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Service.Models.Checkout;
using Shared.Enums.Order;
using Shared.Models.Dtos.Order;
using Shared.Models.Dtos.Order.order;
using Shared.Responses;

namespace Service;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork unitOfWork;

    // Simple settings (later move to appsettings.json)
    private const int ConversionRate = 30;          // 30 points = 1 money unit
    private const int MinimumRedeemPoints = 500;    // minimum points
    private const decimal RedeemCapPercent = 0.20m; // 20% cap
    private const decimal DeliveryFeeFixed = 0.00m;

    public OrderService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public async Task<AppResponse<CheckoutPreviewResponseDto>> PreviewAsync(CheckoutPreviewRequestDto dto,CancellationToken ct)
    {
        // 1) Validate request
        var validation = ValidateCheckoutPreviewRequest(dto);
        if (validation is not null) return validation;

        // 2) Load address snapshot
        var (addressOk, _) = await unitOfWork.Orders.GetAddressSnapshotAsync(dto.AddressId, dto.CustomerId, ct);
        if (!addressOk)
            return AppResponse<CheckoutPreviewResponseDto>.ValidationError("Invalid address");



        // 3) Validate cart source type
        var sourceValidation = await ValidateCartSourceTypeAsync<CheckoutPreviewResponseDto>(
            dto.CustomerId,
            dto.SourceType,
            dto.SourceId,
            ct);

        if (sourceValidation is not null)
            return sourceValidation;



        // 3) Load checkout lines from source
        var lines = await LoadCheckoutLinesAsync(dto.CustomerId, dto.SourceType, dto.SourceId, ct);
        if (lines.Count == 0)
            return AppResponse<CheckoutPreviewResponseDto>.BusinessRuleViolation("Source has no items");

        // 4) Build priced items
        var priced = BuildPricedItems(lines);

        // 5) Handle points redemption preview
        if (dto.SourceType == CheckoutSourceType.PointsRedemption)
        {
            var redeemValidation = ValidatePointsRedemptionLines(lines);
            if (redeemValidation is not null)
                return redeemValidation;

            var subtotal = Round2(priced.Sum(x => x.LineSubtotal));

            var availablePoints = await unitOfWork.Orders.GetCustomerPointsAsync(dto.CustomerId, ct);

            var redeemPreview = CalculateRedeemPreview(subtotal, availablePoints);

            var totals = CalculateTotals(
                priced,
                promoDiscountTotal: 0m,
                redeemedAmount: redeemPreview.RedeemedAmount);

            var response = BuildPointsRedemptionPreviewResponse(dto, priced, totals, redeemPreview);

            return AppResponse<CheckoutPreviewResponseDto>.Ok(
                response,
                "Checkout preview calculated successfully");
        }

        // 6) Calculate promotions for normal checkout
        var promo = await CalculatePromotionsAsync(priced, ct);

        // 7) Apply promotions
        ApplyDiscounts(priced, promo);

        // 8) Calculate earned points preview only
        var earnedPoints = CalculateEarnedPoints(priced);

        // 9) Calculate totals without redeem points
        var normalTotals = CalculateTotals(priced, promo.PromotionDiscountTotal, redeemedAmount: 0m);

        // 10) Build normal response
        var normalResponse = BuildNormalPreviewResponse(dto, priced, promo, normalTotals, earnedPoints);

        return AppResponse<CheckoutPreviewResponseDto>.Ok(
            normalResponse,
            "Checkout preview calculated successfully");

        // Future improvements:
        // - Show customer point balance in normal checkout
        // - Add stock validation if required
    }
    

    public async Task<AppResponse<CheckoutResultDto>> CheckoutAsync(CheckoutRequestDto dto, CancellationToken ct)
    {
        // 1) Validate request before opening transaction
        var validation = ValidateCheckoutRequest(dto);
        if (validation is not null)
            return validation;

        // 2) Open transaction
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 3) Load address snapshot
            var (addressOk, addressSnap) = await unitOfWork.Orders.GetAddressSnapshotAsync(dto.AddressId, dto.CustomerId, ct);
            if (!addressOk)
                return AppResponse<CheckoutResultDto>.ValidationError("Invalid address");


            // 3) Validate cart source type
            var sourceValidation = await ValidateCartSourceTypeAsync<CheckoutResultDto>(
                dto.CustomerId,
                dto.SourceType,
                dto.SourceId,
                ct);

            if (sourceValidation is not null)
                return sourceValidation;


            // 4) Load checkout lines from source
            var lines = await LoadCheckoutLinesAsync(dto, ct);
            if (lines.Count == 0)
                return AppResponse<CheckoutResultDto>.BusinessRuleViolation("Source has no items");

            // 5) Build priced items
            var priced = BuildPricedItems(lines);

            // 6) Handle points redemption checkout
            if (dto.SourceType == CheckoutSourceType.PointsRedemption)
            {
                var redeemValidation = ValidatePointsRedemptionLinesForCheckout(lines);
                if (redeemValidation is not null)
                    return redeemValidation;

                var subtotal = Round2(priced.Sum(x => x.LineSubtotal));

                var availablePoints = await unitOfWork.Orders.GetCustomerPointsAsync(dto.CustomerId, ct);
                var redeemPreview = CalculateRedeemPreview(subtotal, availablePoints);

                if (redeemPreview.RedeemedPoints <= 0)
                    return AppResponse<CheckoutResultDto>.BusinessRuleViolation("Customer does not have enough points to redeem");

                var totals = CalculateTotals(priced, promoDiscountTotal: 0m, redeemedAmount: redeemPreview.RedeemedAmount);

                var points = new PointsCalcResult
                {
                    MaxRedeemablePoints = redeemPreview.MaxRedeemablePoints,
                    RedeemedPoints = redeemPreview.RedeemedPoints,
                    RedeemedAmount = redeemPreview.RedeemedAmount
                };

                var promo = new PromotionCalcResult
                {
                    PromotionDiscountTotal = 0m,
                    AnyPromotionApplied = false
                };

                var order = await CreateOrderAsync(
                    dto,
                    lines,
                    priced,
                    totals,
                    points,
                    addressSnap,
                    promo,
                    earnedPoints: 0,
                    ct);

                await ConsumePointsForOrderAsync(dto.CustomerId, order.Id, points.RedeemedPoints, ct);

                await UpdateSourceAfterCheckoutAsync(dto, lines, ct);

                // 7) Save all pending changes
                await unitOfWork.CompleteAsync(ct);

                // 8) Commit transaction before returning
                await tx.CommitAsync(ct);

                // 9) Build response after commit
                var response = BuildCheckoutResponse(order, priced, totals, promo, points);

                return AppResponse<CheckoutResultDto>.Created(
                    response,
                    location: $"/api/v1/orders/{order.Id}",
                    title: "Order created successfully");
            }

            // 10) Calculate promotions for normal checkout
            var normalPromo = await CalculatePromotionsAsync(priced, ct);

            // 11) Apply promotions
            ApplyDiscounts(priced, normalPromo);

            // 12) Calculate earned points
            var earned = CalculateEarnedPoints(priced);

            // 13) Build normal points result
            var normalPoints = new PointsCalcResult
            {
                MaxRedeemablePoints = 0,
                RedeemedPoints = 0,
                RedeemedAmount = 0m
            };

            // 14) Calculate totals
            var normalTotals = CalculateTotals(priced, normalPromo.PromotionDiscountTotal, redeemedAmount: 0m);

            // 15) Create normal order
            var normalOrder = await CreateOrderAsync(
                dto,
                lines,
                priced,
                normalTotals,
                normalPoints,
                addressSnap,
                normalPromo,
                earnedPoints: earned,
                ct);

            // 16) Create pending earned point lot
            await CreatePendingEarnLotIfNeededAsync(dto.CustomerId, normalOrder.Id, earned, ct);

            // 17) Update source state
            await UpdateSourceAfterCheckoutAsync(dto, lines, ct);

            // 18) Save all pending changes
            await unitOfWork.CompleteAsync(ct);

            // 19) Commit transaction before returning
            await tx.CommitAsync(ct);

            // 20) Build response after commit
            var normalResult = BuildCheckoutResponse(normalOrder, priced, normalTotals, normalPromo, normalPoints);

            return AppResponse<CheckoutResultDto>.Created(
                normalResult,
                location: $"/api/v1/orders/{normalOrder.Id}",
                title: "Order created successfully");
        }
        catch
        {
            // 21) Rollback transaction on any failure
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AppResponse<List<AdminOrderListItemDto>>> GetAdminOrdersAsync(
    AdminOrderListQueryDto query, CancellationToken ct)
    {
        // 1) Validate required fields
        if (query is null || query.StoreId <= 0)
        {
            return AppResponse<List<AdminOrderListItemDto>>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["StoreId"] = new[] { "StoreId is required" }
                },
                detail: "Validation failed");
        }

        // 2) Normalize pagination
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 20;
        if (query.PageSize > 200) query.PageSize = 200;

        // 3) Call repository
        var result = await unitOfWork.Orders.SearchAdminAsync(query, ct);

        // 4) Build pagination
        var pagination = PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount);
        

        // 5) Return response
        return AppResponse<List<AdminOrderListItemDto>>.Ok(
            result.Items,
            pagination,
            "Orders retrieved successfully");

        // Future improvements:
        // - Add caching for frequently accessed queries
        // - Add role-based authorization
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

    private async Task<List<CheckoutLineData>> LoadCheckoutLinesAsync(CheckoutRequestDto dto,CancellationToken ct)
    {
        // 1) Load lines from source
        return dto.SourceType switch
        {
            CheckoutSourceType.Cart =>
                await unitOfWork.Orders.GetLinesFromCartAsync(dto.SourceId, dto.CustomerId, ct),

            CheckoutSourceType.Prescription =>
                await unitOfWork.Orders.GetLinesFromPrescriptionAsync(dto.SourceId, dto.CustomerId, ct),

            CheckoutSourceType.PointsRedemption =>
                await unitOfWork.Orders.GetLinesFromPointsRedemptionCartAsync(dto.SourceId, dto.CustomerId, ct),

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
                DiscountAmount = 0m,
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

        // 3) Mark group promotion items
        foreach (var i in items)
        {
            if (groupByProduct.TryGetValue(i.ProductId, out var gp))
            {
                i.GroupPromotion = gp;
                i.AppliedPromotionId = gp.PromotionId;
            }
        }

        // 4) Calculate exact group discount distribution
        var groupDiscount = CalculateGroupPromotionDiscount(items);

        // 5) Apply product promotion only when not in group promotion
        var productDiscount = 0m;

        foreach (var i in items)
        {
            if (i.GroupPromotion != null)
                continue;

            if (productPromoPerc.TryGetValue(i.ProductId, out var percent) && percent > 0)
            {
                var discount = Round2(i.LineSubtotal * (percent / 100m));

                i.DiscountPercent = percent;
                i.DiscountAmount += discount;

                productDiscount += discount;
            }
        }

        // 6) Return total discount
        var total = Round2(groupDiscount + productDiscount);

        return new PromotionCalcResult
        {
            PromotionDiscountTotal = total,
            AnyPromotionApplied = total > 0m
        };

        // Future improvements:
        // - Support allow points with promotion flag
        // - Support multiple group promotions selection by best discount
    }

    private static decimal CalculateGroupPromotionDiscount(List<PricedCheckoutItem> items)
    {
        // 1) Group items by applied group promotion id
        var groups = items
            .Where(x => x.GroupPromotion != null)
            .GroupBy(x => x.GroupPromotion!.PromotionId);

        var totalDiscount = 0m;

        foreach (var group in groups)
        {
            var promo = group.First().GroupPromotion!;
            var basic = promo.BasicAmount;
            var offer = promo.OfferAmount;

            if (basic <= 0 || offer <= 0)
                continue;

            var bundleSize = basic + offer;
            var totalQty = group.Sum(x => x.Quantity);

            // 2) Calculate discounted units count
            var times = (int)Math.Floor(totalQty / bundleSize);
            var discountedUnits = times * offer;

            if (discountedUnits <= 0)
                continue;

            // 3) Expand units and sort by cheapest first
            var unitRefs = ExpandUnitRefs(group.ToList());

            // 4) Apply discount to cheapest units only
            foreach (var unit in unitRefs.Take(discountedUnits))
            {
                var discount = Round2(unit.Item.UnitPrice * (promo.DiscountPercent / 100m));

                unit.Item.DiscountAmount += discount;
                unit.Item.AppliedPromotionId = promo.PromotionId;

                totalDiscount += discount;
            }
        }

        return Round2(totalDiscount);

        // Future improvements:
        // - Support fractional quantities if business requires it
        // - Support choosing best promotion across entire cart
    }

    private static List<(PricedCheckoutItem Item, decimal UnitPrice)> ExpandUnitRefs(List<PricedCheckoutItem> items)
    {
        // 1) Expand units as item references for discount distribution
        var list = new List<(PricedCheckoutItem Item, decimal UnitPrice)>();

        foreach (var item in items)
        {
            var count = (int)Math.Floor(item.Quantity);

            for (var i = 0; i < count; i++)
                list.Add((item, item.UnitPrice));
        }

        // 2) Sort by cheapest unit first
        return list
            .OrderBy(x => x.UnitPrice)
            .ToList();
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
        // 1) Apply total discount amount per line
        foreach (var i in items)
        {
            if (i.DiscountAmount < 0)
                i.DiscountAmount = 0;

            if (i.DiscountAmount > i.LineSubtotal)
                i.DiscountAmount = i.LineSubtotal;

            i.LineTotal = Round2(i.LineSubtotal - i.DiscountAmount);

            // 2) Compute effective final unit price
            i.FinalUnitPrice = i.Quantity > 0
                ? Round2(i.LineTotal / i.Quantity)
                : 0m;
        }

        // Future improvements:
        // - Return DiscountAmount in response DTO for clearer UI
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
            int earnedPoints,
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

            OrderSource = (byte)dto.SourceType,
            EarnedPoints = earnedPoints,


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

            EarnedPoints = dto.SourceType == CheckoutSourceType.PointsRedemption? 0 : (int)Math.Floor(i.PointsPerUnit * i.Quantity),


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

    private async Task UpdateSourceAfterCheckoutAsync(CheckoutRequestDto dto,List<CheckoutLineData> lines,CancellationToken ct)
    {
        // 1) Mark normal cart as checked out
        if (dto.SourceType == CheckoutSourceType.Cart && lines.First().CartId.HasValue)
            await unitOfWork.Orders.MarkCartCheckedOutAsync(lines.First().CartId!.Value, ct);

        // 2) Close prescription after checkout
        if (dto.SourceType == CheckoutSourceType.Prescription && lines.First().PrescriptionId.HasValue)
            await unitOfWork.Orders.ClosePrescriptionAsync(lines.First().PrescriptionId!.Value, ct);

        // 3) Mark points redemption cart as checked out
        if (dto.SourceType == CheckoutSourceType.PointsRedemption && lines.First().CartId.HasValue)
            await unitOfWork.Orders.MarkCartCheckedOutAsync(lines.First().CartId!.Value, ct);

        // Future improvement:
        // - Split source update logic by source handler
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
                DiscountAmount = i.DiscountAmount,
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

    private static AppResponse<CheckoutPreviewResponseDto>? ValidatePointsRedemptionLines(List<CheckoutLineData> lines)
    {
        // 1) Ensure all lines are redeemable by points
        if (lines.Any(x => !x.IsRedeemableByPoints))
            return AppResponse<CheckoutPreviewResponseDto>.BusinessRuleViolation("All products must be redeemable by points");

        // 2) Ensure all cart lines are points redemption source
        if (lines.Any(x => x.CartItemSourceType != 2))
            return AppResponse<CheckoutPreviewResponseDto>.BusinessRuleViolation("Cart contains non-redemption items");

        return null;
    }

    private static AppResponse<CheckoutResultDto>? ValidatePointsRedemptionLinesForCheckout(List<CheckoutLineData> lines)
    {
        // 1) Ensure all lines are redeemable by points
        if (lines.Any(x => !x.IsRedeemableByPoints))
            return AppResponse<CheckoutResultDto>.BusinessRuleViolation("All products must be redeemable by points");

        // 2) Ensure all cart lines are points redemption source
        if (lines.Any(x => x.CartItemSourceType != 2))
            return AppResponse<CheckoutResultDto>.BusinessRuleViolation("Cart contains non-redemption items");

        return null;
    }

    private static int CalculateEarnedPoints(List<PricedCheckoutItem> items)
    {
        // 1) Calculate earned points from product points
        return items.Sum(x => (int)Math.Floor(x.PointsPerUnit * x.Quantity));

        // Future improvement: support category/store based earning rules
    }


    private async Task CreatePendingEarnLotIfNeededAsync(int customerId,int orderId,int earnedPoints,CancellationToken ct)
    {
        // 1) Skip if no earned points
        if (earnedPoints <= 0) return;

        var now = DateTime.UtcNow;

        // 2) Create pending point lot
        var lot = new CustomerPointLot
        {
            CustomerId = customerId,
            OrderId = orderId,
            OrderItemId = null,

            PointsTotal = earnedPoints,
            RemainingPoints = earnedPoints,

            Status = 1, // 1=Pending
            EarnedAt = now,
            AvailableAt = null,
            ExpiresAt = now.AddYears(1),

            CreatedAt = now
        };

        await unitOfWork.Orders.AddCustomerPointLotAsync(lot, ct);

        // 3) Save lot to get id
        await unitOfWork.CompleteAsync(ct);

        // 4) Create earn pending transaction
        var tx = new CustomerPointTransaction
        {
            CustomerId = customerId,
            LotId = lot.Id,
            OrderId = orderId,

            TransactionType = 1, // 1=EarnPending
            PointsDelta = earnedPoints,
            BalanceAfter = null,

            ExpiresAt = lot.ExpiresAt,
            ReferenceType = 1, // 1=Order
            ReferenceId = orderId,

            CreatedAt = now
        };

        await unitOfWork.Orders.AddPointTransactionsRangeAsync(new[] { tx }, ct);

        // Future improvement: create one lot per order item if needed
    }


    private async Task ConsumePointsForOrderAsync(int customerId,int orderId,int pointsToConsume,CancellationToken ct)
    {
        // 1) Skip if no points to consume
        if (pointsToConsume <= 0) return;

        var now = DateTime.UtcNow;

        // 2) Load available lots in FEFO order
        var lots = await unitOfWork.Orders.GetAvailablePointLotsForUpdateAsync(customerId, now, ct);

        var available = lots.Sum(x => x.RemainingPoints);
        if (available < pointsToConsume)
            throw new InvalidOperationException("Insufficient available points");

        var remaining = pointsToConsume;
        var transactions = new List<CustomerPointTransaction>();

        // 3) Consume points from lots
        foreach (var lot in lots)
        {
            if (remaining <= 0) break;

            var take = Math.Min(lot.RemainingPoints, remaining);

            lot.RemainingPoints -= take;
            lot.UpdatedAt = now;

            if (lot.RemainingPoints == 0)
                lot.Status = 3; // 3=Used

            remaining -= take;

            transactions.Add(new CustomerPointTransaction
            {
                CustomerId = customerId,
                LotId = lot.Id,
                OrderId = orderId,

                TransactionType = 3, // 3=Redeem
                PointsDelta = -take,
                PointsPerEGP = ConversionRate,
                AmountEGP = Round2(take / (decimal)ConversionRate),

                ReferenceType = 1, // 1=Order
                ReferenceId = orderId,

                CreatedAt = now
            });
        }

        // 4) Decrease fast customer balance
        var affected = await unitOfWork.Orders.DecreaseCustomerPointsAsync(customerId, pointsToConsume, ct);
        if (affected == 0)
            throw new InvalidOperationException("Unable to decrease customer points");

        // 5) Add ledger transactions
        await unitOfWork.Orders.AddPointTransactionsRangeAsync(transactions, ct);

        // Future improvement: add BalanceAfter after final customer balance is known
    }

    private async Task<AppResponse<T>?> ValidateCartSourceTypeAsync<T>(int customerId,CheckoutSourceType checkoutSourceType,int cartId,CancellationToken ct)
    {
        // 1) Skip validation for prescription source
        if (checkoutSourceType == CheckoutSourceType.Prescription)
            return null;

        // 2) Load cart source types
        var sourceTypes = await unitOfWork.Orders.GetCartItemSourceTypesAsync(cartId, customerId, ct);

        if (sourceTypes.Count == 0)
            return AppResponse<T>.BusinessRuleViolation("Cart has no items");

        // 3) Validate normal cart checkout
        if (checkoutSourceType == CheckoutSourceType.Cart)
        {
            if (sourceTypes.Any(x => x != 1))
                return AppResponse<T>.BusinessRuleViolation(
                    "This cart contains points redemption items. Use PointsRedemption checkout source.");
        }

        // 4) Validate points redemption checkout
        if (checkoutSourceType == CheckoutSourceType.PointsRedemption)
        {
            if (sourceTypes.Any(x => x != 2))
                return AppResponse<T>.BusinessRuleViolation(
                    "This cart contains normal items. Use Cart checkout source.");
        }

        return null;
    }

    private static CheckoutPreviewResponseDto BuildNormalPreviewResponse(
        CheckoutPreviewRequestDto dto,
        List<PricedCheckoutItem> priced,
        PromotionCalcResult promo,
        TotalsCalcResult totals,
        int earnedPoints)
    {
        // 1) Build normal checkout preview response
        return new CheckoutPreviewResponseDto
        {
            SourceType = dto.SourceType,
            SourceId = dto.SourceId,

            Subtotal = totals.Subtotal,
            PromotionDiscountTotal = totals.PromotionDiscountTotal,
            SubtotalAfterPromotions = totals.SubtotalAfterPromotions,

            MaxRedeemablePoints = 0,
            RequestedRedeemPoints = 0,
            AppliedRedeemPoints = 0,
            RedeemValueMoney = 0,

            DeliveryFee = totals.DeliveryFee,
            GrandTotal = totals.GrandTotal,

            HasPriceChanges = false,
            Warnings = promo.PromotionDiscountTotal > 0
                ? new List<string> { "Promotions are applied. Points redemption is not allowed in this checkout." }
                : new List<string>(),

            Items = priced.Select(i => new CheckoutPreviewItemDto
            {
                ProductId = i.ProductId,
                UnitLevel = i.UnitLevel,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                DiscountAmount = i.DiscountAmount,
                FinalUnitPrice = i.FinalUnitPrice,
                LineTotal = i.LineTotal,
                AppliedPromotionId = i.GroupPromotion?.PromotionId,
                SnapshotPrice = null
            }).ToList()
        };
    }


    private static CheckoutPreviewResponseDto BuildPointsRedemptionPreviewResponse(
        CheckoutPreviewRequestDto dto,
        List<PricedCheckoutItem> priced,
        TotalsCalcResult totals,
        PointsCalcResult redeem)
    {
        // 1) Build points redemption preview response
        return new CheckoutPreviewResponseDto
        {
            SourceType = dto.SourceType,
            SourceId = dto.SourceId,

            Subtotal = totals.Subtotal,
            PromotionDiscountTotal = 0,
            SubtotalAfterPromotions = totals.Subtotal,

            MaxRedeemablePoints = redeem.MaxRedeemablePoints,
            RequestedRedeemPoints = redeem.RedeemedPoints,
            AppliedRedeemPoints = redeem.RedeemedPoints,
            RedeemValueMoney = redeem.RedeemedAmount,

            DeliveryFee = totals.DeliveryFee,
            GrandTotal = totals.GrandTotal,

            HasPriceChanges = false,
            Warnings = redeem.RedeemedPoints == 0
                ? new List<string> { "Customer does not have enough points to redeem." }
                : new List<string>(),

            Items = priced.Select(i => new CheckoutPreviewItemDto
            {
                ProductId = i.ProductId,
                UnitLevel = i.UnitLevel,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercent = 0,
                FinalUnitPrice = i.UnitPrice,
                LineTotal = i.LineSubtotal,
                AppliedPromotionId = null,
                SnapshotPrice = null
            }).ToList()
        };
    }


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

    private async Task<List<CheckoutLineData>> LoadCheckoutLinesAsync(int customerId,CheckoutSourceType sourceType,int sourceId,CancellationToken ct)
    {
        // 1) Load lines from source
        return sourceType switch
        {
            CheckoutSourceType.Cart =>
                await unitOfWork.Orders.GetLinesFromCartAsync(sourceId, customerId, ct),

            CheckoutSourceType.Prescription =>
                await unitOfWork.Orders.GetLinesFromPrescriptionAsync(sourceId, customerId, ct),

            CheckoutSourceType.PointsRedemption =>
                await unitOfWork.Orders.GetLinesFromPointsRedemptionCartAsync(sourceId, customerId, ct),

            _ => new List<CheckoutLineData>()
        };
    }

    private static PointsCalcResult CalculateRedeemPreview(decimal subtotal, int availablePoints)
    {
        // 1) Stop if customer does not meet minimum points
        if (availablePoints < MinimumRedeemPoints)
        {
            return new PointsCalcResult
            {
                MaxRedeemablePoints = availablePoints,
                RedeemedPoints = 0,
                RedeemedAmount = 0m
            };
        }

        // 2) Convert available points to money
        var availableAmount = Round2(availablePoints / (decimal)ConversionRate);

        // 3) Use max possible amount
        var redeemAmount = Math.Min(subtotal, availableAmount);

        // 4) Convert redeem amount back to points
        var redeemedPoints = (int)Math.Floor(redeemAmount * ConversionRate);

        // 5) Apply minimum on used points
        if (redeemedPoints < MinimumRedeemPoints)
        {
            return new PointsCalcResult
            {
                MaxRedeemablePoints = redeemedPoints,
                RedeemedPoints = 0,
                RedeemedAmount = 0m
            };
        }

        return new PointsCalcResult
        {
            MaxRedeemablePoints = redeemedPoints,
            RedeemedPoints = redeemedPoints,
            RedeemedAmount = Round2(redeemedPoints / (decimal)ConversionRate)
        };
    }

    // order status
    public async Task<AppResponse<int>> UpdateOrderStatusAsync(int orderId,OrderStatusUpdateDto dto,CancellationToken ct)
    {
        // 1) Validate input
        if (orderId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid order id" }
                },
                detail: "Validation failed");
        }

        if (dto is null || dto.Status < 1 || dto.Status > 8)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Status"] = new[] { "Status must be between 1 and 8" }
                },
                detail: "Validation failed");
        }

        // 2) Load order
        var order = await unitOfWork.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
        if (order is null)
            return AppResponse<int>.NotFound("Order not found");

        // 3) Ignore if order already has the requested status
        if (order.Status == dto.Status)
            return AppResponse<int>.Ok(order.Id, "Order status is already updated");

        // 4) Validate status transition
        if (!IsOrderTransitionAllowed(order.Status, dto.Status))
            return AppResponse<int>.BusinessRuleViolation("Invalid order status transition");

        // 5) Apply status update
        var now = DateTime.UtcNow;

        order.Status = dto.Status;
        order.StatusUpdatedAt = now;
        order.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            order.Notes = dto.Notes.Trim();

        // 6) Make earned points available when order is delivered
        if (dto.Status == 5) // 5=Delivered
        {
            var pointsResult = await MakeOrderPointsAvailableAsync(order, now, ct);
            if (pointsResult is not null)
                return pointsResult;
        }

        // 7) Save changes
        unitOfWork.Orders.UpdateOrder(order);
        await unitOfWork.CompleteAsync(ct);

        // 8) Return success
        return AppResponse<int>.Ok(order.Id, "Order status updated successfully");

        // Future improvements:
        // - Wrap this method in a database transaction
        // - Add order status history table
        // - Send notification after status update
    }

    private static bool IsOrderTransitionAllowed(byte current, byte next)
    {
        // 1) Define simple order state machine
        return (current, next) switch
        {
            (1, 2) => true, // Pending -> Confirmed
            (2, 3) => true, // Confirmed -> Preparing
            (3, 4) => true, // Preparing -> OutForDelivery
            (4, 5) => true, // OutForDelivery -> Delivered

            (1, 6) => true, // Pending -> Cancelled
            (2, 6) => true, // Confirmed -> Cancelled
            (3, 6) => true, // Preparing -> Cancelled

            (5, 7) => true, // Delivered -> Returned

            (8, 2) => true, // PendingApproval -> Confirmed

            _ => false
        };

        // Future improvement: make transitions configurable from database
    }

    private async Task<AppResponse<int>?> MakeOrderPointsAvailableAsync(Order order,DateTime nowUtc,CancellationToken ct)
    {
        // 1) Skip points for points redemption orders
        if (order.OrderSource == 3) // 3=PointsRedemption
            return null;

        // 2) Load pending lots for this order
        var lots = await unitOfWork.Orders.GetPendingPointLotsByOrderAsync(order.Id, ct);

        // 3) Skip if no pending earned points
        if (lots.Count == 0)
            return null;

        // 4) Calculate total points to activate
        var totalPoints = lots.Sum(x => x.RemainingPoints);

        if (totalPoints <= 0)
            return null;

        // 5) Load customer for points update
        var customer = await unitOfWork.Orders.GetCustomerForPointsUpdateAsync(order.CustomerId, ct);
        if (customer is null)
            return AppResponse<int>.NotFound("Customer not found");

        // 6) Mark lots as available
        foreach (var lot in lots)
        {
            lot.Status = 2; // 2=Available
            lot.AvailableAt = nowUtc;
            lot.UpdatedAt = nowUtc;
        }

        // 7) Increase customer fast balance
        customer.Points += totalPoints;

        // 8) Create EarnAvailable transactions
        var transactions = lots.Select(lot => new CustomerPointTransaction
        {
            CustomerId = order.CustomerId,
            LotId = lot.Id,
            OrderId = order.Id,
            OrderItemId = lot.OrderItemId,

            TransactionType = 2, // 2=EarnAvailable
            PointsDelta = lot.RemainingPoints,
            BalanceAfter = null,

            PointsPerEGP = null,
            AmountEGP = null,

            ExpiresAt = lot.ExpiresAt,
            ReferenceType = 1, // 1=Order
            ReferenceId = order.Id,

            SourceTransactionId = null,
            CreatedByEmployeeId = null,
            Notes = "Earned points became available after delivery",

            CreatedAt = nowUtc
        }).ToList();

        await unitOfWork.Orders.AddPointTransactionsRangeAsync(transactions, ct);

        return null;

        // Future improvements:
        // - Fill BalanceAfter after computing final customer balance
        // - If you create multiple lots per order, adjust unique transaction index to avoid conflicts
    }

}
