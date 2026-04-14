namespace Shared.Models.Dtos.Cart;


public sealed class CartPreviewItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }

    public byte UnitLevel { get; set; }     // 1=Outer,2=Inner
    public decimal Quantity { get; set; }

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }

    public string? PrimaryImageUrl { get; set; } // null => UI shows default image

    public decimal CurrentUnitPrice { get; set; }         // Latest from product
    public decimal CurrentUnitPriceSnapshot { get; set; } // Saved at add/update

    public decimal LineTotal { get; set; }                // qty * current price

    public decimal AvailableQty { get; set; }
    public bool ExceedsAvailableQty { get; set; }

    public bool IsInGroupPromotion { get; set; }          // For future group promotions
}
