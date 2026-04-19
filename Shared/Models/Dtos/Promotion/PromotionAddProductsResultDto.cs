namespace Shared.Models.Dtos.Promotion;

public sealed class PromotionAddProductsResultDto
{
    public int PromotionId { get; set; }
    public int RequestedCount { get; set; }
    public int AddedCount { get; set; }
    public int RestoredCount { get; set; }
    public int SkippedDuplicateCount { get; set; }

    public List<decimal> SkippedErpProductIds { get; set; } = new();
}