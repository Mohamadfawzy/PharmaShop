using Shared.Enums;

namespace Shared.Models.RequestFeatures;

public class ProductParameters: RequestParameters
{
    // Filtering
    public int? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsIntegrated { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public bool? IsGroupOffer { get; set; }

    // Search
    public string? Name { get; set; }
    public string? Barcode { get; set; }
    public string? StockProductCode { get; set; }

    public ProductOrderBy OrderBy { get; set; } = ProductOrderBy.CreatedAt;

}
