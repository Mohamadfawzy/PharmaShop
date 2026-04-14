using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class CartPreviewResponseDto
{
    public int CartId { get; set; }
    public bool CanProceed { get; set; }

    public decimal Subtotal { get; set; }
    public decimal PromotionDiscountTotal { get; set; }
    public decimal SubtotalAfterPromotions { get; set; }

    public int MaxRedeemablePoints { get; set; }
    public int RequestedRedeemPoints { get; set; }
    public int AppliedRedeemPoints { get; set; }
    public decimal RedeemValueMoney { get; set; }

    public decimal DeliveryFee { get; set; }
    public decimal GrandTotal { get; set; }

    public bool HasPriceChanges { get; set; } // Warning only
    public List<string> Warnings { get; set; } = new();

    public List<CartPreviewItemDto> Items { get; set; } = new();
}
