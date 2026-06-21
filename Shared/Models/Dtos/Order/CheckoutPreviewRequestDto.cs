using Shared.Enums.Cart;
using Shared.Enums.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order;


public sealed class CheckoutPreviewRequestDto
{
    public int CustomerId { get; set; }           // For now (later: from token)
    public CheckoutSourceType SourceType { get; set; }
    public int SourceId { get; set; }             // CartId or PrescriptionId
    public int AddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
}

public sealed class CheckoutPreviewResponseDto
{
    public CheckoutSourceType SourceType { get; set; }
    public int SourceId { get; set; }

    public decimal Subtotal { get; set; }
    public decimal PromotionDiscountTotal { get; set; }
    public decimal SubtotalAfterPromotions { get; set; }

    public int MaxRedeemablePoints { get; set; }
    public int RequestedRedeemPoints { get; set; }
    public int AppliedRedeemPoints { get; set; }
    public decimal RedeemValueMoney { get; set; }

    public decimal DeliveryFee { get; set; }
    public decimal GrandTotal { get; set; }

    public bool HasPriceChanges { get; set; }     // Warning only
    public List<string> Warnings { get; set; } = new();

    public List<CheckoutPreviewItemDto> Items { get; set; } = new();
}

public sealed class CheckoutPreviewItemDto
{
    public int ProductId { get; set; }
    public byte UnitLevel { get; set; }           // 1=Outer,2=Inner
    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }        // Current unit price
    public decimal DiscountPercent { get; set; }  // 0..100
    public decimal DiscountAmount { get; set; }
    public decimal FinalUnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public int? AppliedPromotionId { get; set; }  // Group promotion applied (if any)

    public decimal? SnapshotPrice { get; set; }   // Optional: if coming from Cart snapshot
}
