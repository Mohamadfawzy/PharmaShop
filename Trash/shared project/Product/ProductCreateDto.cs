namespace Shared.Models.Dtos.Product;

public partial class ProductCreateDto
{
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }

    // Unified codes (optional but unique per pharmacy when provided)
    public string? Barcode { get; init; }
    public string? InternationalCode { get; init; }
    public string? StockProductCode { get; init; }

    // Names
    public string Name { get; init; } = default!;
    public string NameEn { get; init; } = default!;

    // Optional text
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? DescriptionEn { get; init; }

    // Search helpers
    public string? SearchKeywords { get; init; }

    // Packaging info (informational)
    public string? DosageForm { get; init; }
    public string? Strength { get; init; }
    public string? PackSize { get; init; }
    public string? Unit { get; init; }

    // Compliance / rules
    public bool RequiresPrescription { get; init; } = false;
    public bool EarnPoints { get; init; } = true;
    public bool HasExpiry { get; init; } = true;

    public bool AgeRestricted { get; init; } = false;
    public int? MinAge { get; init; }

    public bool RequiresColdChain { get; init; } = false;
    public bool ControlledSubstance { get; init; } = false;
    public string? StorageConditions { get; init; }

    // Taxes
    public bool IsTaxable { get; init; } = true;
    public decimal VatRate { get; init; } = 0m;
    public string? TaxCategoryCode { get; init; }

    // Order limits
    public int MinOrderQty { get; init; } = 1;
    public int? MaxOrderQty { get; init; }
    public int? MaxPerDayQty { get; init; }

    public bool IsReturnable { get; init; } = true;
    public int? ReturnWindowDays { get; init; }

    // Split sale policy
    public bool AllowSplitSale { get; init; } = false;
    public byte? SplitLevel { get; init; } // 1=Inner(Strip) | 2=Base(Tablet)

    // Shipping / dimensions
    public int? WeightGrams { get; init; }
    public int? LengthMm { get; init; }
    public int? WidthMm { get; init; }
    public int? HeightMm { get; init; }

    // Flags
    public bool TrackInventory { get; init; } = true;
    public bool IsFeatured { get; init; } = false;

    // You can allow create as inactive if you want (optional):
    public bool IsActive { get; init; } = true;
}
