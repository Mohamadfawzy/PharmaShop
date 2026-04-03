using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public byte UnitLevel { get; set; }

    public decimal Quantity { get; set; }

    public int OuterUnitIdSnapshot { get; set; }

    public int? InnerUnitIdSnapshot { get; set; }

    public int? InnerPerOuterSnapshot { get; set; }

    public decimal UnitPriceSnapshot { get; set; }

    public decimal DiscountPercentSnapshot { get; set; }

    public decimal FinalUnitPriceSnapshot { get; set; }

    public decimal LineTotal { get; set; }

    public int PointsSnapshot { get; set; }

    public int EarnedPoints { get; set; }

    public int? AppliedPromotionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Promotion? AppliedPromotion { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
