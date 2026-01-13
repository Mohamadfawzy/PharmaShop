using Shared.Enums.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.RequestFeatures;

public sealed class ProductListQueryDto
{
    // -------------------------
    // Paging
    // -------------------------
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // -------------------------
    // Sorting
    // -------------------------
    public ProductSortBy SortBy { get; init; } = ProductSortBy.CreatedAt;
    public SortDirection SortDir { get; init; } = SortDirection.Desc;

    // -------------------------
    // Soft delete visibility
    // -------------------------
    // includeDeleted=true => يعرض deleted + non-deleted
    public bool IncludeDeleted { get; init; } = false;

    // onlyDeleted=true => يعرض deleted فقط (له أولوية على IncludeDeleted)
    public bool OnlyDeleted { get; init; } = false;

    // -------------------------
    // Search
    // -------------------------
    // Search keyword (name/code/keywords)
    public string? Q { get; init; }
    public ProductSearchMode SearchMode { get; init; } = ProductSearchMode.Contains;

    // Fast exact searches (useful for scanning)
    public string? Barcode { get; init; }
    public string? InternationalCode { get; init; }
    public string? StockProductCode { get; init; }

    // -------------------------
    // Filters (IDs)
    // -------------------------
    public int? CategoryId { get; init; }
    public List<int>? CategoryIds { get; init; }

    public int? BrandId { get; init; }
    public List<int>? BrandIds { get; init; }

    // -------------------------
    // Filters (Flags)
    // -------------------------
    public bool? IsActive { get; init; }
    public bool? RequiresPrescription { get; init; }
    public bool? EarnPoints { get; init; }
    public bool? HasExpiry { get; init; }

    public bool? AgeRestricted { get; init; }
    public int? MinAgeFrom { get; init; }
    public int? MinAgeTo { get; init; }

    public bool? RequiresColdChain { get; init; }
    public bool? ControlledSubstance { get; init; }

    public bool? IsTaxable { get; init; }
    public decimal? VatRateFrom { get; init; }
    public decimal? VatRateTo { get; init; }
    public string? TaxCategoryCode { get; init; }

    public bool? TrackInventory { get; init; }
    public bool? IsFeatured { get; init; }
    public bool? AllowSplitSale { get; init; }
    public byte? SplitLevel { get; init; } // 1/2

    // -------------------------
    // Filters (Ranges / Dates)
    // -------------------------
    public DateTime? CreatedFromUtc { get; init; }
    public DateTime? CreatedToUtc { get; init; }

    public DateTime? UpdatedFromUtc { get; init; }
    public DateTime? UpdatedToUtc { get; init; }

    // -------------------------
    // Future-proof include options (optional)
    // -------------------------
    // مثال: لاحقاً نضيف include units / images / stock summary
    public bool IncludePrimaryImage { get; init; } = false;


    // -------------------------
    // Stock options
    // -------------------------
    public bool IncludeStockSummary { get; init; } = false; // يعرض qty + flags

    public bool InStockOnly { get; init; } = false;         // shortcut => StockStatus=InStock
    public StockStatus StockStatus { get; init; } = StockStatus.Any;

    // Optional store filter (لو تريد مخزن محدد)
    public int? StoreId { get; init; }

    // Optional thresholds
    public int? MinAvailableQty { get; init; }              // مثال: >= 5
}
