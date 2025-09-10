namespace Shared.Models.Dtos.Product;

public class ProductSubDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }

    public decimal? Points { get; set; }

    public decimal? PromoDisc { get; set; }

    public DateTime? PromoEndDate { get; set; }

    public bool IsGroupOffer { get; set; }

    public string ImageName { get; set; } = string.Empty;
}
