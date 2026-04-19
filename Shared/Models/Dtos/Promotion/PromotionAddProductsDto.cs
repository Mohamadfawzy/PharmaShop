namespace Shared.Models.Dtos.Promotion;


public sealed class PromotionAddProductsDto
{
    public List<PromotionProductCreateItemDto> Items { get; set; } = new();
}

public sealed class PromotionProductCreateItemDto
{
    public int? ProductId { get; set; }           // Local ProductId (optional)
    public decimal ErpProductId { get; set; }     // Required by schema
    public decimal? ErpOfferId { get; set; }      // Optional
}
