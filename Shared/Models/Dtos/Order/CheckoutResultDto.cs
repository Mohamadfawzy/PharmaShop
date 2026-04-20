using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order;


public sealed class CheckoutResultDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;

    public decimal Subtotal { get; set; }
    public decimal PromotionDiscountTotal { get; set; }
    public decimal SubtotalAfterPromotions { get; set; }

    public int RedeemedPoints { get; set; }
    public decimal RedeemedAmount { get; set; }

    public decimal DeliveryFee { get; set; }
    public decimal GrandTotal { get; set; }

    public List<CheckoutItemResultDto> Items { get; set; } = new();
}
