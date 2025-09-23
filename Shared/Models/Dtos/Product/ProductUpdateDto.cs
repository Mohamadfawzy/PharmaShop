namespace Shared.Models.Dtos.Product;

public class ProductUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    //public string Barcode { get; set; } = null!;
    //public decimal Price { get; set; }
    //public decimal? OldPrice { get; set; }
    //public int? CategoryId { get; set; }
    //public int? SubCategoryId { get; set; }
    //public bool IsAvailable { get; set; } = true;
}
