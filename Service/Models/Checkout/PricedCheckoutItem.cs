using Shared.Models.Dtos.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Checkout;

internal sealed class PricedCheckoutItem
{
    public int ProductId { get; set; }
    public byte UnitLevel { get; set; }
    public decimal Quantity { get; set; }

    public int OuterUnitIdSnapshot { get; set; }
    public int? InnerUnitIdSnapshot { get; set; }
    public int? InnerPerOuterSnapshot { get; set; }
    public int? AppliedPromotionId { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal LineSubtotal { get; set; }

    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalUnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public int PointsPerUnit { get; set; }

    public ActiveGroupPromotionHit? GroupPromotion { get; set; }
}