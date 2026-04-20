namespace Shared.Models.Dtos.Order;


public sealed class ActiveGroupPromotionHit
{
    public int PromotionId { get; set; }
    public int BasicAmount { get; set; }
    public int OfferAmount { get; set; }
    public decimal DiscountPercent { get; set; }
}
