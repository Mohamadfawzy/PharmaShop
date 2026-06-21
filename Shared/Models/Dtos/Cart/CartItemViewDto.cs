using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;

public sealed class CartItemViewDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }

    public byte UnitLevel { get; set; }          // 1=Outer,2=Inner
    public decimal Quantity { get; set; }

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }

    public string? PrimaryImageUrl { get; set; } // null => UI shows default image

    public decimal? CurrentUnitPrice { get; set; }          // From Products (latest)
    public decimal CurrentUnitPriceSnapshot { get; set; }   // From CartItems (saved at add/update)

    public decimal AvailableQty { get; set; }               // In the same UnitLevel
    public bool ExceedsAvailableQty { get; set; }           // qty > available

    public bool IsRedeemableByPoints { get; set; }
    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsValid { get; set; }                       // Stored value (may be stale)
    public string? InvalidReason { get; set; }              // Stored value
}
