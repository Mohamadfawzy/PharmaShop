using Shared.Enums.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class AddToCartResponseDto
{
    public int CartId { get; set; }
    public CartStatus CartStatus { get; set; }

    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public UnitLevel UnitLevel { get; set; }

    public CartItemSourceType SourceType { get; set; }


    public decimal Quantity { get; set; }

    public decimal AvailableQty { get; set; }           // Current available qty in the same unit level
    public bool ExceedsAvailableQty { get; set; }       // True if requested qty > available

    public bool RequiresRefresh { get; set; }           // True when price changed and cart set to Invalid
    public string? RefreshReason { get; set; }          // Simple message for UI

    public ItemPriceChangeDto? PriceChange { get; set; } // Old/New price details when changed
}

public sealed class ItemPriceChangeDto
{
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}