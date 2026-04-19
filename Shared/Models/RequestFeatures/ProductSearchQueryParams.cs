using Shared.Enums.Product;

namespace Shared.Models.RequestFeatures;


public sealed class ProductSearchQueryParams
{
    // Store scope (recommended)
    public int? StoreId { get; set; }

    // Search term (Arabic or English)
    public string? Q { get; set; }

    // Filters
    public int? CategoryId { get; set; }
    public int? CompanyId { get; set; }
    public bool? RequiresPrescription { get; set; }
    public bool? HasPromotion { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsActive { get; set; }

    // Price range (outer unit)
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Tags filter
    // Use repeated query: tagIds=1&tagIds=2
    public List<int>? TagIds { get; set; }
    public TagMatchMode TagMatch { get; set; } = TagMatchMode.Any;

    // Sorting
    public ProductSortOption Sort { get; set; } = ProductSortOption.NameAsc;

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

