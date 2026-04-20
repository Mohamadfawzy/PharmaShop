using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Checkout;

internal sealed class PromotionCalcResult
{
    public decimal PromotionDiscountTotal { get; set; }
    public bool AnyPromotionApplied { get; set; }
}