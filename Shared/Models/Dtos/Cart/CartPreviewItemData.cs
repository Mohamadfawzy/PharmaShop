namespace Shared.Models.Dtos.Cart;



public sealed class CartPreviewItemData
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }

    public byte UnitLevel { get; set; }
    public decimal Quantity { get; set; }
    public decimal CurrentUnitPriceSnapshot { get; set; }

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public decimal OuterUnitPrice { get; set; }
    public decimal? InnerUnitPrice { get; set; }
    public int? InnerPerOuter { get; set; }

    public decimal StockOuterQty { get; set; } // Products.Quantity stored in OUTER

    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool HasPromotion { get; set; }
    public decimal PromotionDiscountPercent { get; set; }
    public DateTime? PromotionStartsAt { get; set; }
    public DateTime? PromotionEndsAt { get; set; }

    public int Points { get; set; } // 0 => excluded from points
    public bool AllowSplitSale { get; set; }

    public bool IsInGroupPromotion { get; set; } // Hook: set by join with PromotionProducts
}
