namespace Shared.Models.Dtos.Promotion;

public class PublicPromotionListItemDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public int? TotalAmount { get; init; }
    public int? BasicAmount { get; init; }
    public int? OfferAmount { get; init; }
    public decimal? DiscountPercent { get; init; }
}

public sealed class PublicPromotionDetailsDto : PublicPromotionListItemDto
{
    public List<int> ProductIds { get; init; } = [];
}
