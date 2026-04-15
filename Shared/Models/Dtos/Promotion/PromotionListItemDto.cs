namespace Shared.Models.Dtos.Promotion;

public sealed class PromotionListItemDto
{
    public int Id { get; set; }
    public decimal? ErpPgoId { get; set; }

    public string? Name { get; set; }
    public bool IsActive { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public int? TotalAmount { get; set; }
    public int? BasicAmount { get; set; }
    public int? OfferAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public DateTime CreatedAt { get; set; }
}