using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order;


public sealed class CheckoutItemResultDto
{
    public int ProductId { get; set; }
    public byte UnitLevel { get; set; }               // 1=Outer,2=Inner
    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }            // Current unit price
    public decimal DiscountPercent { get; set; }      // 0..100
    public decimal FinalUnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public int? AppliedPromotionId { get; set; }      // Group promotion applied (if any)
}

