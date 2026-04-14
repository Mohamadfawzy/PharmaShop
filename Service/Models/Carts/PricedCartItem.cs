using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Carts;

internal sealed class PricedCartItem
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public byte UnitLevel { get; set; }
    public decimal Quantity { get; set; }

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? PrimaryImageUrl { get; set; }

    public decimal CurrentUnitPrice { get; set; }
    public decimal CurrentUnitPriceSnapshot { get; set; }

    public decimal AvailableQty { get; set; }
    public bool ExceedsAvailableQty { get; set; }

    public decimal LineTotal { get; set; }

    public bool HasProductPromotion { get; set; }
    public decimal ProductPromotionPercent { get; set; }
    public bool IsInGroupPromotion { get; set; }

    public int Points { get; set; }
}