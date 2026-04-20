using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Checkout;

internal sealed class TotalsCalcResult
{
    public decimal Subtotal { get; set; }
    public decimal PromotionDiscountTotal { get; set; }
    public decimal SubtotalAfterPromotions { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal GrandTotal { get; set; }
}