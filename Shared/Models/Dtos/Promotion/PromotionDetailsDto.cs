namespace Shared.Models.Dtos.Promotion;


public sealed class PromotionDetailsDto
{
    public int Id { get; set; }
    public decimal? ErpPgoId { get; set; }

    public string? Name { get; set; }
    public string? Notes { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public int? TotalAmount { get; set; }
    public int? BasicAmount { get; set; }
    public int? OfferAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public bool IsActive { get; set; }

    public int ProductsCount { get; set; }
    public List<PromotionProductItemDto> Products { get; set; } = new();
}

