namespace Shared.Models.Dtos.Product;

public class ProductUpdateDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string Barcode { get; set; } = null!;
    public string? InternationalCode { get; set; }
    public string? StockProductCode { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsIntegrated { get; set; }
    public string? CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public decimal? Points { get; set; }
    public decimal? PromoDisc { get; set; }
}
