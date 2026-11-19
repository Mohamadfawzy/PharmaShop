namespace Shared.Models.Dtos.Product;

public sealed class ProductDetailsDto
{
    public int Id { get; init; }
    public int StoreId { get; init; }
    public int CategoryId { get; init; }
    public int? CompanyId { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string? NameEn { get; init; }
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? InternationalCode { get; init; }
    public string? SearchKeywords { get; init; }
    public int OuterUnitId { get; init; }
    public int? InnerUnitId { get; init; }
    public int? InnerPerOuter { get; init; }
    public decimal OuterUnitPrice { get; init; }
    public decimal? InnerUnitPrice { get; init; }
    public decimal Quantity { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsActive { get; init; }
    public bool RequiresPrescription { get; init; }
    public bool HasPromotion { get; init; }
    public decimal PromotionDiscountPercent { get; init; }
    public DateTime? PromotionStartsAt { get; init; }
    public DateTime? PromotionEndsAt { get; init; }
    public bool IsFeatured { get; init; }
    public bool HasExpiry { get; init; }
    public int Points { get; init; }
    public List<string> Images { get; init; } = [];
    public List<int> TagIds { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
