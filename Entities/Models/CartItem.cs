using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public byte UnitLevel { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPriceSnapshot { get; set; }

    public decimal CurrentUnitPriceSnapshot { get; set; }

    public int MinOrderQtySnapshot { get; set; }

    public int? MaxOrderQtySnapshot { get; set; }

    public int? MaxPerDayQtySnapshot { get; set; }

    public bool IsValid { get; set; }

    public string? InvalidReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
